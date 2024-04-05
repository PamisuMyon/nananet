namespace Nananet.Adapter.Fanbook.Api;

public abstract class BaseApi
{
    protected RestHandler RestHandler { get; private set; }

    protected BaseApi(RestHandler restHandler)
    {
        RestHandler = restHandler;
    }
}