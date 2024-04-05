namespace Nananet.Adapter.Fanbook.Api;

public class BotApi : BaseApi
{
    public BotApi(RestHandler restHandler) : base(restHandler)
    {
    }

    public Task GetMeAsync()
    {
        return RestHandler.GetAsync("getMe");
    }
    
}