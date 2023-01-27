using MongoDB.Bson;
using MongoDB.Entities;
using Nananet.App.Nana.Storage;
using Nananet.Core.Utils;
using Newtonsoft.Json.Linq;

namespace Nananet.App.Nana.Models.Ak;

[Collection("handbooks")]
public class Handbook : Entity
{
    public string CharID { get; set; }
    public string Ability { get; set; }
    public string Birthday { get; set; }
    public string Birthplace { get; set; }
    public string Condition { get; set; }
    public string DrawName { get; set; }
    public string Experience { get; set; }
    public string Gender { get; set; }
    // public List<object> HandbookAvgList { get; set; }
    public string Height { get; set; }
    public string InfoName { get; set; }
    public string Name { get; set; }
    public string Race { get; set; }
    public List<StoryTextAudio> StoryTextAudio { get; set; }
    public Dictionary<string, CV> CvDictionary { get; set; }


    public static Task<Handbook?> FindOneByName(string name, bool fuzzy)
    {
        return DbUtil.FindOneByField<Handbook>("name", name, fuzzy, true);
    }

    public static async Task<List<Handbook>> FindByBirthday(int month, int date)
    {
        var query = new BsonDocument();
        if (date == -1)
            query.Add("birthday", new BsonDocument
            {
                { "$regex", $"^{month}月.+" },
            });
        else
            query.Add("birthday", $"{month}月{date}日");
        
        return await DB.Find<Handbook>()
            .Match(query)
            .ExecuteAsync();
    }


    public static async Task<string?> GetBirthdayMessageSimple(DateTime dateTime)
    {
        var ops = await FindByBirthday(dateTime.Month, dateTime.Day);
        if (ops is not { Count: > 0 }) return null;
        var msg = "";
        for (var i = 0; i < ops.Count; i++)
        {
            if (string.IsNullOrEmpty(ops[i].Name)) continue;
            msg += ops[i].Name;
            if (i != ops.Count - 1)
                msg += "、";
        }

        if (!string.IsNullOrEmpty(msg))
            msg = "🎂今天生日的干员：" + msg;
        return msg;
    }

    public static async Task<string?> GetBirthDayMessage(int month, int date, string? dayStr = null)
    {
        var ops = await FindByBirthday(month, date);
        var msg = "";
        for (var i = 0; i < ops.Count; i++)
        {
            if (ops[i].Name.NotNullNorEmpty())
            {
                msg += ops[i].Name;
                if (i != ops.Count - 1)
                    msg += "、";
            }
        }
        if (msg.NotNullNorEmpty())
        {
            if (dayStr.NotNullNorEmpty()) 
            {
                msg = $"🎂{dayStr}生日的干员：{msg}"; 
            }
            else
            {
                if (date == -1)
                    msg = $"🎂{month}月生日的干员：{msg}";
                else
                    msg = $"🎂{month}月{date}日生日的干员：{msg}";
            }

        }
        else
        {
            if (dayStr.NotNullNorEmpty()) 
            {
                msg = $"{dayStr}没有干员过生日哦。"; 
            }
            else
            {
                if (date == -1)
                    msg = $"{month}月没有干员过生日哦。";
                else
                    msg = $"{month}月{date}日没有干员过生日哦。";
            }
        }
        return msg;
    }
}

public class CV
{
    public string Wordkey { get; set; }
    public string VoiceLangType { get; set; }
    public string[] CvName { get; set; }
}

public class Story
{
    public string StoryText { get; set; }
    public int UnLockType { get; set; }
    public string UnLockParam { get; set; }
    public string UnLockString { get; set; }
}

public class StoryTextAudio
{
    public List<Story> Stories { get; set; }
    public string StoryTitle { get; set; }
    public bool UnLockorNot { get; set; }
}
