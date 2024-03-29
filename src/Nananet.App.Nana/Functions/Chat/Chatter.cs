using System.Text.RegularExpressions;
using Nananet.App.Nana.Models;
using Nananet.Core.Utils;
using RestSharp;

namespace Nananet.App.Nana.Functions.Chat;

public class Chatter
{
    public struct ReplyResult
    {
        public string Content;
        public string Priority;
    }

    protected static Regex _puncRegex =
        new(
            "[\\ \\~\\`\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)\\-\\+\\=\\|\\\\[\\]\\{\\}\\;\\:\"\'\\,\\<\\.\\>\\/\\?《》【】「」￥！。，”“、…]",
            RegexOptions.Multiline);

    protected static Regex _emojiRegex = new("(\ud83c[\udf00-\udfff])|(\ud83d[\udc00-\ude4f])|(\ud83d[\ude80-\udeff])",
        RegexOptions.Multiline);

    public static string Wash(string content)
    {
        var str = _puncRegex.Replace(content, "");
        str = _emojiRegex.Replace(str, "");
        if (!string.IsNullOrEmpty(str.Trim()))
        {
            return str;
        }

        return content;
    }

    public static ReplyResult? GetConversationReply(string content)
    {
        foreach (var item in Conversation.Cache.Value)
        {
            if (item.Type == "sentence")
            {
                // 完整句子匹配
                var found = false;
                foreach (var q in item.Q)
                {
                    if (content == q)
                    {
                        found = true;
                        if (item.Condition != "and")
                        {
                            break;
                        }
                    }
                    else if (item.Condition == "and")
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    // 优先级最高，立即返回结果
                    return new ReplyResult
                    {
                        Content = item.A.RandomElem(),
                        Priority = "2"
                    };
                }
            }
            else if (item.Type == "regex")
            {
                // 正则匹配
                var found = false;
                foreach (var q in item.Q)
                {
                    var reg = new Regex(q, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (reg.IsMatch(content))
                    {
                        found = true;
                        if (item.Condition != "and")
                        {
                            break;
                        }
                    }
                    else if (item.Condition == "and")
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return new ReplyResult
                    {
                        Content = item.A.RandomElem(),
                        Priority = item.Priority
                    };
                }
            }
        }

        return null;
    }

    public static async Task<NanaChatResult?> RequestNanaChat(string content)
    {
        var url = $"http://127.0.0.1:7700/api/v1/chat?msg={content}";
        var options = new RestClientOptions(url)
        {
            MaxTimeout = 2000,
            ThrowOnAnyError = true
        };
        var client = new RestClient(options);

        var request = new RestRequest();
        try
        {
            var response = await client.ExecuteGetAsync(request);
            return JsonUtil.FromJson<NanaChatResult>(response.Content!);
        }
        catch (Exception e)
        {
            Logger.L.Error("エラー発生: GetNanaChat");
            Logger.L.Error(e.Message);
        }

        return null;
    }

    public struct NanaChatResult
    {
        public string reply;
        public float confidence;
    }
}