using DiscountNews.Communication;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using RestSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiscountNews
{
    internal class Program
    {
        private readonly IEmailService _email;
        public Program(IEmailService email)
        {
            _email = email;
        }
        static void Main(string[] args)
        {


            Console.WriteLine("Ürün linkini giriniz");
            var url = Console.ReadLine();
            url = UrlCheck(url);

            Console.WriteLine("Email adresinizi giriniz");
            var email = Console.ReadLine();
            email = IsValidEmail(email);

            Console.WriteLine("Kaç TL'nin altına düşünce mail ile bilgilendirilmek istersiniz?");
            var priceControl = Console.ReadLine();
            var price = Convert.ToDecimal(IsNumber(priceControl));

            Console.WriteLine("Kaç saat aralıkla kontrol edilsin");
            var durationControl = Console.ReadLine();
            var duration = Convert.ToDecimal(IsNumber(durationControl));

            try
            {
                // Grab the Scheduler instance from the Factory 
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

                // and start it off
                scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .UsingJobData("email", email)
                    .UsingJobData("url", url)
                    .UsingJobData("price", price.ToString())
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever()
                        )
                    .Build();

                // Tell quartz to schedule the job using our trigger
                scheduler.ScheduleJob(job, trigger);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                // and last shut down the scheduler when you are ready to close your program
                //scheduler.Shutdown();
                Console.ReadLine();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }
        public class HelloJob : IJob
        {
            Task IJob.Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                string email = dataMap.GetString("email");
                string url = dataMap.GetString("url");
                decimal price = Convert.ToDecimal(dataMap.GetString("price"));

                Uri urlFirst = new Uri(url);
                var host = urlFirst.Host;
                var uri = urlFirst.AbsolutePath;

                var clientLive = new RestClient("https://" + host);
                var requestLive = new RestRequest(uri);
                var responseLive = clientLive.GetAsync(requestLive)?.Result.Content;
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(responseLive);

                decimal? amountValue = null;
                var amountValueString = htmlDocument.DocumentNode.SelectNodes("//meta[@name='twitter:data1']")[0]?.Attributes[1]?.Value.Replace(".", ",");
                var title = htmlDocument.DocumentNode.SelectNodes("//h1[contains(@class, 'pr-new-br')]")[0]?.InnerText;
                amountValue = Convert.ToDecimal(amountValueString);
                if (price > amountValue)
                {
                    var serviceProvider = new ServiceCollection()
                          .AddSingleton<IEmailService, EmailService>()
                          .BuildServiceProvider();
                    var emailService = serviceProvider.GetService<IEmailService>();
                    emailService.SendEmail(email, title, url);
                }
                return Task.CompletedTask;
            }
        }
        public static string UrlCheck(string url)
        {
            while (true)
            {
                Uri uriResult;
                bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    return url;
                }
                Console.WriteLine("Url is wrong please try again");
                url = Console.ReadLine();
            }

        }
        public static string IsValidEmail(string email)
        {
            var emailControl = new EmailAddressAttribute();
            while (true)
            {
                if (emailControl.IsValid(email))
                    return email;
                Console.WriteLine("Re-enter your email address");
                email = Console.ReadLine();
            }
        }
        public static string IsNumber(string value)
        {
            while (true)
            {
                Regex regex = new Regex("[0-9]");
                if (regex.IsMatch(value))
                {
                    return value;
                }
                Console.WriteLine("Please enter number only.");
                value = Console.ReadLine();
            }

        }
    }
}
