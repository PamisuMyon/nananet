using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.Models.Results;

namespace Nananet.Sdk.Fanbook.Api;

public class GuildApi : BaseApi
{
    public GuildApi(RestHandler restHandler) : base(restHandler)
    {
    }

    public async Task<List<Guild>?> GetMyGuildsAsync()
    {
        var result = await RestHandler.PostAsync<CommonResult<GetMyGuildsResult>>("guild/myGuild2", "");
        if (result?.Status == true)
            return result.Data?.Lists;
        return null;
    }
    
}