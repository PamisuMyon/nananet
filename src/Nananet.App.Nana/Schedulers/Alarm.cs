using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Utils;
using Quartz;
using Quartz.Impl;

namespace Nananet.App.Nana.Schedulers;

public class Alarm
{
    protected IBot _bot;

    public Alarm(IBot bot)
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
        job.JobDataMap.Add("bot", _bot);

        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("22 0 0 * * ?")
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        Logger.L.Info("Alarm scheduled.");
    }

    class BirthdayJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap.Get("bot") is not IBot bot)
            {
                Logger.L.Error("Cannot get bot from context.");
                return;
            }

            try
            {
                var content = await Handbook.GetBirthdayMessageSimple(DateTime.Now);
                if (string.IsNullOrEmpty(content))
                {
                    Logger.L.Debug("No birthday message for now.");
                    return;
                }

                foreach (var it in bot.Config.Channels)
                {
                    if (it.Key == "all") continue;
                    if (it.Value.AlarmBirthday)
                    {
                        await bot.SendTextMessage(it.Key, content, false);
                        Logger.L.Info($"Birthday message sent to {it.Key} : {content}");
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Logger.L.Error("Birthday Job Error.");
                Logger.L.Error(e.Message);
            }
        }
    }
}