using Nado.Core;
using Nado.Core.Utils;
using Quartz;
using Quartz.Impl;

namespace Nado.App.Nana.Schedulers;

public class Alarm
{
    protected NadoBot _bot;

    public Alarm(NadoBot bot)
    {
        _bot = bot;
    }

    public async void Schedule()
    {
        var factory = new StdSchedulerFactory();
        var scheduler = await factory.GetScheduler();
        await scheduler.Start();

        var job = JobBuilder.Create<BirthdayJob>()
            .WithIdentity("birthday", "group")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("22 0 0 * * ?")
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        Logger.L.Info("Alarm scheduled.");
    }

    class BirthdayJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }

}