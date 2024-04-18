using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.Models.Results;
using Newtonsoft.Json.Linq;

namespace Nananet.Sdk.Fanbook.Api;

public class UserApi : BaseApi
{
    public UserApi(RestHandler restHandler) : base(restHandler)
    {
    }

    public async Task<JObject?> GetUserSettingAsync()
    {
        var result = await RestHandler.PostAsync<CommonResult<JObject>>("userSetting/get", "");
        if (result?.Status == true)
        {
            return result.Data;
        }

        return null;
    }

    public async Task<List<User>?> GetUsersAsync(params string[] userIds)
    {
        var jo = new JObject();
        var ja = new JArray();
        for (var i = 0; i < userIds.Length; i++)
        {
            ja.Add(userIds[i]);
        }
        jo["user_ids"] = ja;
        jo["guild_id"] = null;
        jo["transaction"] = Guid.NewGuid().ToString();

        var result = await RestHandler.PostAsync<CommonResult<List<User>>>("user/getUser", jo.ToString());
        if (result?.Status == true)
            return result.Data;
        return null;
    }

    public async Task<User?> GetMeAsync()
    {
        var userSetting = await GetUserSettingAsync();
        if (userSetting != null && userSetting.TryGetValue("user_id", out var id))
        {
            var users = await GetUsersAsync(id.Value<string>()!);
            if (users != null)
                return users[0];
        }

        return null;
    }
    
}