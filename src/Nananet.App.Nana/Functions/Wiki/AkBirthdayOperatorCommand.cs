using System.Text.RegularExpressions;
using Nananet.App.Nana.Commons;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Wiki;

public class AkBirthdayOperatorCommand : Command
{
    public override string Name => "wiki/birthdayOperator";

    private Regex _regex1 = new("(大前天|前天|昨天|今天|明天|后天|大后天)是?谁?过?生日(的干员)?");
    private Regex _regex2 = new("(\\d+)(月|\\.)(\\d+)?(日|号)?是?谁?过?生日(的干员)?");
    private Regex _regex3 = new("([零一二三四五六七八九十百千万亿兆京垓]+)(月|\\.)([零一二三四五六七八九十百千万亿兆京垓]+)?(日|号)?是?谁?过?生日(的干员)?");
    private Dictionary<string, int> _dateDict = new()
    {
        { "大前天", -3 },
        { "前天", -2 },
        { "昨天", -1 },
        { "今天", 0 },
        { "明天", 1 },
        { "后天", 2 },
        { "大后天", 3 }
    };

    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return NoConfidence;

        Match m;
        string? reply = null;
        if (_regex1.IsMatch(input.Content))
        {
            m = _regex1.Match(input.Content);
            if (_dateDict.ContainsKey(m.Groups[1].Value))
            {
                var offset = _dateDict[m.Groups[1].Value];
                var date = DateTime.Now;
                date = date.AddDays(offset);
                reply = await Handbook.GetBirthDayMessage(date.Month, date.Day, m.Groups[1].Value);
            }
        }
        else if (_regex2.IsMatch(input.Content))
        {
            m = _regex2.Match(input.Content);
            var bMonth = int.TryParse(m.Groups[1].Value, out var month);
            var bDay = int.TryParse(m.Groups[3].Value, out var day);
            if ((bMonth && bDay && month >= 1 && month <= 12 && day == -1)
                || NaUtil.IsValidMonthDate(month, day))
                reply = await Handbook.GetBirthDayMessage(month, day);
            else
                reply = Sentence.GetOne("dateError");
        }
        else if (_regex3.IsMatch(input.Content))
        {
            m = _regex3.Match(input.Content)!;
            var month = (int)ZhDigitToArabic.Convert(m.Groups[1].Value);
            var day = -1;
            if (m.Groups[3].Success)
                day = (int)ZhDigitToArabic.Convert(m.Groups[3].Value);

            if (month >= 1 && month <= 12 && ((day >= 1 && day <= 31) || day == -1))
                reply = await Handbook.GetBirthDayMessage(month, day);
            else
                reply = Sentence.GetOne("dateError");
        }

        if (reply.NotNullNorEmpty())
            return new CommandTestInfo
            {
                Confidence = 1,
                Data = reply
            };

        return NoConfidence;
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not string reply) return Failed;
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, input, reply);
        return Executed;
    }
    
}