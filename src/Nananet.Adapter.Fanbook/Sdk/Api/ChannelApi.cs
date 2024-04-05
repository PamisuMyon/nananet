﻿using Newtonsoft.Json.Linq;

namespace Nananet.Adapter.Fanbook.Api;

public class ChannelApi : BaseApi
{
    public ChannelApi(RestHandler restHandler) : base(restHandler)
    {
    }

    public Task ChannelListAsync(string guildId)
    {
        var resJo = new JObject();
        resJo["guild_id"] = guildId;
        return RestHandler.PostAsync("channel/list", resJo.ToString());
    }
}