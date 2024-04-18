using COSXML.Model.Bucket;
using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.Models.Params;
using Nananet.Sdk.Fanbook.Models.Results;
using Nananet.Sdk.Fanbook.Utils;
using Newtonsoft.Json.Linq;

namespace Nananet.Sdk.Fanbook.Api;

public class MessageApi : BaseApi
{

    private int _shardId;
    
    public MessageApi(RestHandler restHandler) : base(restHandler)
    {
        _shardId = Random.Shared.Next(0, 32);
    }
    
    public async Task<ActionResult<ClientSendResult>?> ClientSendAsync(string guildId, string channelId, string contentJson, string desc)
    {
        var url = $"message/clientSend/{guildId}/{channelId}";
        var clientSend = new ClientSendParam()
        {
            GuildId = guildId,
            ChannelId = channelId,
            Content = contentJson,
            Desc = desc,
            Nonce = MessageNonce(),
            Token = RestHandler.RuntimeData.Config.Token,
            Transaction = Guid.NewGuid().ToString()
        };
        var json = RestHandler.ToJson(clientSend);
        return await RestHandler.PostAsync<ActionResult<ClientSendResult>>(url, json);
    }
    
    private string MessageNonce()
    {
        var bb = new DateTimeOffset(2019, 8, 8, 8, 8, 8, TimeSpan.FromHours(8)).ToUnixTimeMilliseconds();
        return Snowflake.Generate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - bb, _shardId);
    }

    public async Task<List<Message>?> GetListAsync(string channelId)
    {
        var param = new GetMessageListParam()
        {
            Size = 50,
            ChannelId = channelId,
            MessageId = null,
            Behavior = "before",
            Transaction = Guid.NewGuid().ToString()
        };
        var json = RestHandler.ToJson(param);
        var result = await RestHandler.PostAsync<CommonResult<List<Message>>>("message/getList", json);
        if (result?.Status == true)
            return result.Data;
        return null;
    }

    public async Task<bool> RecallAsync(string channelId, string messageId)
    {
        var jo = new JObject();
        jo["channel_id"] = channelId;
        jo["message_id"] = messageId;
        jo["transaction"] = Guid.NewGuid().ToString();
        var json = jo.ToString();
        var result = await RestHandler.PostAsync<CommonResult<List<Message>>>("message/recall", json);
        return result?.Status == true;
    }
    
}