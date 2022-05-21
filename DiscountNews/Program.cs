
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscountNews
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ürün linkini giriniz");
            var url = Console.ReadLine();
            Console.WriteLine("Email adresinizi giriniz");
            var email = Console.ReadLine();
            Console.WriteLine("Kaç TL'nin altına düşünce mail ile bilgilendirilmek istersiniz?");
            var price = Convert.ToDecimal(Console.ReadLine());

            try
            {
                // Grab the Scheduler instance from the Factory 
                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

                // and start it off
                scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .UsingJobData("email",email)
                    .UsingJobData("url", url)
                    .UsingJobData("price",price.ToString())
                    .WithIdentity("job1", "group1")
                    .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10)
                        .RepeatForever())
                    .Build();

                // Tell quartz to schedule the job using our trigger
                scheduler.ScheduleJob(job, trigger);
                Thread.Sleep(TimeSpan.FromSeconds(60));
                // and last shut down the scheduler when you are ready to close your program
                scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }
        public class HelloJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                Console.WriteLine("Greetings from HelloJob!");
            }

            Task IJob.Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                string email = dataMap.GetString("email");
                string url = dataMap.GetString("url");
                string price = dataMap.GetString("price");
                return Task.CompletedTask;
            }
        }
    }
}
