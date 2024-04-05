using Nananet.Adapter.Fanbook.Sdk.Models.Params;
using Nananet.Adapter.Fanbook.Utils;
using Newtonsoft.Json.Linq;

namespace Nananet.Adapter.Fanbook.Api;

public class MessageApi
{

    private RestHandler _restHandler;
    
    public MessageApi(RestHandler restHandler)
    {
        _restHandler = restHandler;
    }

    public Task GetUpdatesV2Async()
    {
        return _restHandler.PostAsync("getUpdates", "{}");
    }

    public Task GetMessage(long channelId, long messageId)
    {
        var resJo = new JObject();
        resJo["chat_id"] = channelId;
        resJo["message_id"] = messageId;
        var json = resJo.ToString();
        return _restHandler.PostAsync("getMessage", json);
    }

    public async Task SendMessageAsync(long chatId, string text)
    {
        // var param = new SendMessage
        // {
        //     ChatId = chatId,
        //     Desc = text,
        //     Text = text
        // };
        // var json = _restHandler.ToJson(param);
        var resJo = new JObject();
        resJo["chat_id"] = chatId;
        resJo["text"] = "{\"type\":\"richText\",\"title\":\"签到成功\",\"document\":\"[{\\\"insert\\\":\\\"恭喜你签到成功\\\"}]\"}";
        resJo["parse_mode"] = "Fanbook";
        resJo["reply_to_message_id"] = 616842751761186816;
        resJo["nonce"] = MessageNonce();
        var json = resJo.ToString();
        await _restHandler.PostAsync("sendMessage", json);
    }

    public Task SendPhotoAsync(long chatId)
    {
        var resJo = new JObject();
        resJo["chat_id"] = chatId;
        var photoJo = new JObject();
        photoJo["Url"] = "http://fb-cdn.fanbook.mobi/fanbook/app/files/chatroom/image/afa1a29c055fc15eb9b6a8c3f88ddb54.jpeg";
        resJo["photo"] = photoJo;
        var json = resJo.ToString();
        return _restHandler.PostAsync("sendPhoto", json);
    }
    
    private string MessageNonce()
    {
        var bb = new DateTimeOffset(2019, 8, 8, 8, 8, 8, TimeSpan.FromHours(8)).ToUnixTimeMilliseconds();
        return Snowflake.Generate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - bb, 25);
    }
    
}