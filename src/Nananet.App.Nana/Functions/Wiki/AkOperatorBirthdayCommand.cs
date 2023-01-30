using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.App.Nana.Models.Ak;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Functions.Wiki;

public class AkOperatorBirthdayCommand : Command
{
    public override string Name => "wiki/operatorBirthday";

    private Regex _regex = new("(.+?) *　*在?(哪天)?((什么|啥)时候)?的?过?生日");
    
    public override async Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (!input.HasContent()) return NoConfidence;
        if (_regex.IsMatch(input.Content))
        {
            var m = _regex.Match(input.Content);
            var c = await Handbook.FindOneByName(m.Groups[1].Value, false);
            if (c != null && c.Birthday.NotNullNorEmpty())
            {
                return new CommandTestInfo
                {
                    Confidence = 1,
                    Data = $"🎂{c.Birthday}"
                };
            }
        }
        return NoConfidence;
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        if (testInfo.Data is not string reply) return Failed;
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }
    
}