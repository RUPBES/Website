using Quartz;
using Quartz.Impl;

namespace rupbes.Jobs.TendersLoader
{
    public class UpdateSheduler
    {
        //метод, запускающий таймер
        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            //регистрация класса Updater, как работы
            IJobDetail job = JobBuilder.Create<Updater>().Build();

            //создаем и настраиваем триггер, запускающий работу - класс Updater
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(604800).RepeatForever())
                .Build();

            //указываем планировщику какую работу и когда запускать
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}