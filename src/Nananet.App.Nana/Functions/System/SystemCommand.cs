using System.Text.RegularExpressions;
using Nananet.App.Nana.Commons;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Schedulers;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.System;

public abstract class SystemCommand : Command
{
    public override string Name { get; }
    protected abstract Regex[] Regexes { get; }
    protected abstract string[] Roles { get; }
    protected abstract string[] PlatformRoles { get; }
    protected virtual bool CheckPlatformRole { get; set; } = false;

    public override Task Init(IBot bot)
    {
        // 目前仅有QQ频道可判断用户的角色
        if (CheckPlatformRole &&
            bot.AppSettings.Platform != Constants.PlatfromQQGuild)
            CheckPlatformRole = false;
        return base.Init(bot);
    }

    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return NoConfidence;
        foreach (var regex in Regexes)
        {
            if (!regex.IsMatch(input.Content)) continue;
            // 如果允许，先检查用户是否符合平台角色，如果不符合，再检查用户配置表
            var roleCheck = false;
            if (CheckPlatformRole && input.Member.HasAnyRole(PlatformRoles))
                roleCheck = true;
            else
            {
                // 是否在用户配置表中
                var user = await BotUser.FindById(input.Author.Id);
                if (user != null)
                    roleCheck = Roles.Contains(user.Role);
            }
            if (!roleCheck) continue;
            
            var m = regex.Match(input.Content);
            return new CommandTestInfo
            {
                Confidence = 1,
                Data = m
            };
        }
        return NoConfidence;
    }
}

public class RefreshCacheCommand : SystemCommand
{
    protected override Regex[] Regexes { get; } =
    {
        new("^更新缓存$"),
        new("^更新緩存$"),
    };

    protected override string[] Roles { get; } =
    {
        BotUser.RoleAdmin, BotUser.RoleModerator
    };
    protected override string[] PlatformRoles { get; }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        await bot.Refresh();
        const string reply = "缓存已更新，喵喵喵~";
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
        return Executed;
    }
}

public class AlarmSettingsCommand : SystemCommand
{
    protected override Regex[] Regexes { get; } =
    {
        new("^(开启|打开|启用|关闭|停用)生日提醒$"),
        new("^(開啟|打開|啟用|關閉|停用)生日提醒$"),
    };

    protected override string[] Roles { get; } =
    {
        BotUser.RoleAdmin, BotUser.RoleModerator
    };
    protected override string[] PlatformRoles { get; } =
    {
        Constants.QQGuildRoleAdmin,
        Constants.QQGuildRoleCreator,
        Constants.QQGuildRoleChannelAdmin,
    };
    protected override bool CheckPlatformRole { get; set; } = true;

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not Match m) return Failed;

        var toggleOn = new Regex("(开启|打开|启用|開啟|打開|啟用)").IsMatch(m.Groups[1].Value);
        var configName = bot.AppSettings.Platform + "AlarmConfig";
        var config = await MiscConfig.FindByName<AlarmConfig>(configName);
        config ??= new AlarmConfig();

        AlarmConfig.ChannelConfig channelConfig;
        if (config.Channels.ContainsKey(input.ChannelId))
            channelConfig = config.Channels[input.ChannelId];
        else
        {
            channelConfig = new AlarmConfig.ChannelConfig();
            config.Channels.Add(input.ChannelId, channelConfig);
        }
        channelConfig.AlarmBirthday = toggleOn;
        channelConfig.ChannelId = input.ChannelId;
        channelConfig.GroupId = input.GroupId;
        await MiscConfig.Upsert(configName, config);

        var reply = toggleOn ? "干员生日提醒已开启" : "干员生日提醒已关闭";
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
        return Executed;
    }
    
}