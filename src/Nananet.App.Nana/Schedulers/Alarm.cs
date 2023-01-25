using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Utils;
using Quartz;
using Quartz.Impl;

namespace Nananet.App.Nana.Schedulers;

public class Alarm
{
    private static Alarm? s_instance;
    public static Alarm Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new Alarm();
            return s_instance;
        }
    }
    private Alarm()
    {
        if (s_instance != null)
            throw new Exception("Instance already exists.");
    }

    private IBot _bot;
    protected IScheduler? _scheduler;

    public async Task Schedule(IBot bot)
    {
        _bot = bot;
        if (_scheduler != null)
            await _scheduler.Shutdown();
        
        var factory = new StdSchedulerFactory();
        _scheduler = await factory.GetScheduler();
        await _scheduler.Start();

        var job = JobBuilder.Create<BirthdayJob>()
            .WithIdentity("birthday", "group")
            .Build();
        job.JobDataMap.Add("bot", _bot);

        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("22 0 0 * * ?")
            .Build();

        await _scheduler.ScheduleJob(job, trigger);

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
                var config = await MiscConfig.FindByName<AlarmConfig>(bot.AppSettings.Platform + "AlarmConfig");
                if (config == null)
                    return;

                var content = await Handbook.GetBirthdayMessageSimple(DateTime.Now);
                if (string.IsNullOrEmpty(content))
                {
                    Logger.L.Debug("No birthday message for now.");
                    return;
                }

                foreach (var id in config.Channels.Keys)
                {
                    if (config.Channels[id].AlarmBirthday)
                    {
                        await bot.SendTextMessage(id, content, false);
                        Logger.L.Info($"Birthday message sent to {id} : {content}");
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Logger.L.Error($"Birthday Job Error: {e.Message}");
                Logger.L.Error(e.StackTrace);
            }
        }
    }
    
}

public class AlarmConfig
{
    public Dictionary<string, ChannelConfig> Channels = new();
    
    public class ChannelConfig
    {
        public string ChannelId { get; set; }
        public string GroupId { get; set; }
        public bool AlarmBirthday { get; set; }
    }
}
