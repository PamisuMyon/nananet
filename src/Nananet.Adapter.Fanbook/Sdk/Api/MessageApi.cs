using Nananet.Adapter.Fanbook.Params;
using Nananet.Adapter.Fanbook.Utils;

namespace Nananet.Adapter.Fanbook.Api;

public class MessageApi
{

    private BaseApi _baseApi;
    private int _shardId;
    
    public MessageApi(BaseApi baseApi)
    {
        _baseApi = baseApi;
        _shardId = Random.Shared.Next(0, 32);
    }
    
    public async Task ClientSendAsync(string guildId, string channelId, string contentJson, string desc)
    {
        var url = $"/api/message/clientSend/{guildId}/{channelId}";
        var clientSend = new ClientSend()
        {
            GuildId = guildId,
            ChannelId = channelId,
            Content = contentJson,
            Desc = desc,
            Nonce = MessageNonce(),
            Token = _baseApi.Token,
            Transaction = Guid.NewGuid().ToString()
        };
        var json = _baseApi.ToJson(clientSend);
        await _baseApi.PostAsync(url, json, $"{BaseApi.ApiHost}/channels/{guildId}/{channelId}");
    }
    
    private string MessageNonce()
    {
        var bb = new DateTimeOffset(2019, 8, 8, 8, 8, 8, TimeSpan.FromHours(8)).ToUnixTimeMilliseconds();
        return Snowflake.Generate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - bb, _shardId);
    }
    
}