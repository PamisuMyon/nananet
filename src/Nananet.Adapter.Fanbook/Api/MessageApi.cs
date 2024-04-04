using Nananet.Adapter.Fanbook.Params;

namespace Nananet.Adapter.Fanbook.Api;

public class MessageApi
{

    private BaseApi _baseApi;
    
    public MessageApi(BaseApi baseApi)
    {
        _baseApi = baseApi;
    }
    
    public async Task ClientSendAsync(string guildId, string channelId, string contentJson, string desc)
    {
        var url = $"a1/api/message/clientSend/{guildId}/{channelId}";
        var clientSend = new ClientSend()
        {
            GuildId = guildId,
            ChannelId = channelId,
            Content = contentJson,
            Desc = desc,
            Nonce = _baseApi.Nonce().ToString(),
            Token = _baseApi.Token,
            Transaction = Guid.NewGuid().ToString()
        };
        var json = _baseApi.ToJson(clientSend);
        await _baseApi.PostAsync(url, json, "https://web.fanbook.cn/channels/616200178189582337/616200820257841152");
    }
    
}