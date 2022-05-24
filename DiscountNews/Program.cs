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


            Console.WriteLine("Enter the product url");
            var url = Console.ReadLine();
            url = UrlCheck(url);

            Console.WriteLine("Enter your email address");
            var email = Console.ReadLine();
            email = IsValidEmail(email);

            Console.WriteLine("If the price of the product falls below how much TRY, should the notification e-mail be sent?");
            var priceControl = Console.ReadLine();
            var price = Convert.ToDecimal(IsNumber(priceControl));

            Console.WriteLine("How many hours to check?");
            var durationControl = Console.ReadLine();
            var duration = Convert.ToInt32(IsNumber(durationControl));

            try
            {
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
                scheduler.Start();
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .UsingJobData("email", email)
                    .UsingJobData("url", url)
                    .UsingJobData("price", price.ToString())
                    .WithIdentity("job1", "group1")
                    .Build();
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(duration)
                        .RepeatForever())
                    .Build();
                scheduler.ScheduleJob(job, trigger);
                Thread.Sleep(TimeSpan.FromSeconds(5));
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
