using Nananet.Adapter.Fanbook.Models;
using Nananet.Adapter.Fanbook.Params;
using Nananet.Adapter.Fanbook.Sdk.Models.Results;
using Nananet.Adapter.Fanbook.Utils;

namespace Nananet.Adapter.Fanbook.Api;

public class MessageApi : BaseApi
{

    private int _shardId;
    
    public MessageApi(RestHandler restHandler) : base(restHandler)
    {
        _shardId = Random.Shared.Next(0, 32);
    }
    
    public async Task ClientSendAsync(string guildId, string channelId, string contentJson, string desc)
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
        await RestHandler.PostAsync<ActionResult>(url, json);
    }
    
    private string MessageNonce()
    {
        var bb = new DateTimeOffset(2019, 8, 8, 8, 8, 8, TimeSpan.FromHours(8)).ToUnixTimeMilliseconds();
        return Snowflake.Generate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - bb, _shardId);
    }

    public async Task<List<Message>?> GetListAsync(string channelId)
    {
        const string url = "message/getList";
        var param = new GetMessageListParam()
        {
            Size = 50,
            ChannelId = channelId,
            MessageId = null,
            Behavior = "before",
            Transaction = Guid.NewGuid().ToString()
        };
        var json = RestHandler.ToJson(param);
        var result = await RestHandler.PostAsync<CommonResult<List<Message>>>(url, json);
        if (result?.Status == true)
            return result.Data;
        return null;
    }
    
}