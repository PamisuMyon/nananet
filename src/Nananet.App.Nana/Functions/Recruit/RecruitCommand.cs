using System.Text.RegularExpressions;
using Nananet.App.Nana.Functions.AI;
using Nananet.App.Nana.Models;
using Nananet.Core;
using Nananet.Core.Commands;
using Nananet.Core.Models;

namespace Nananet.App.Nana.Functions.Recruit;

public class RecruitCommand : Command
{
    public override string Name => "recruit"; 
    
    protected Regex[] _regexes =
    {
        new ("(公招|公开招募)(查询)?"),
        new ("(公招|公開招募)(查詢)?"),
    };
    
    public override Task<CommandTestInfo> Test(Message input, CommandTestOptions options)
    {
        if (input.HasContent())
        {
            if (_regexes.Any(regex => regex.IsMatch(input.Content))
                || input.HasAttachment())
                return Task.FromResult(FullConfidence);
        }
        return Task.FromResult(NoConfidence);
    }

    public override async Task<CommandResult> Execute(IBot bot, Message input, CommandTestInfo testInfo)
    {
        string reply;
        if (input.HasAttachment())
            // TODO 目前仅支持消息中同时包含指令与图片
            reply = await DoOcrRecruit(input.Attachments![0].Url);
        else
            reply = await DoTextRecruit(input.Content);
        await bot.ReplyTextMessage(input, reply);
        await ActionLog.Log(Name, bot, input, reply);
        return Executed;
    }

    private async Task<string> DoOcrRecruit(string imageUrl)
    {
        var words = await BaiduOcr.Execute(imageUrl);
        if (words == null || words.Count == 0)
        {
            if (BaiduOcr.LimitReached)
                return Sentence.GetOne("ocrLimited");
            return Sentence.GetOne("ocrError");
        }
        
        // 剔除不存在的tag
        var allTags = Recruiter.Instance.Tags;
        List<string> tags = new();
        for (var i = words.Count - 1; i >= 0; i--) {
            var word = words[i].Trim();
            word = Correct(word);
            if (!string.IsNullOrEmpty(word) && allTags.Contains(word)) {
                tags.Add(word);
            }
        }
        if (tags.Count == 0) 
            return Sentence.GetOne("ocrError")!;
        if (tags.Count > 7)    // hard-code
            // 剔除多余的tag 
            tags = tags.Take(10).ToList();

        // 根据tag计算
        var results = await Recruiter.Instance.Calculate(tags);
        var reply = "🔍识别到的标签：\n";
        foreach (var tag in tags) {
            reply += tag + " ";
        }
        reply += "\n\n";
        reply += Recruiter.Instance.BeautifyRecruitResults(results);
        return reply;
    }
    
    private static string Correct(string word)
    {
        word = word.Replace("于员", "干员");
        return word;
    }

    private async Task<string> DoTextRecruit(string content)
    {
        var split = content!.Replace("　", " ").Trim().Split(" ").ToList();
        // 剔除不存在的tag
        var allTags = Recruiter.Instance.Tags;
        for (var i = split.Count - 1; i >= 0; i--) {
            if (split[i].Trim().Length == 0 || !allTags.Contains(split[i].Trim()))
            {
                split.RemoveAt(i);
            }
        }
        if (split.Count == 0) 
            return Sentence.GetOne("recruitNoTagError");
        if (split.Count > 7) // hard-code
            return Sentence.GetOne("recruitToManyTagsError");
            
        // 根据tag计算
        var results = await Recruiter.Instance.Calculate(split);
        var reply = "🔍" + Recruiter.Instance.BeautifyRecruitResults(results);
        return reply;
    }
    
}