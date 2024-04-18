using Nananet.Adapter.Fanbook.Sdk.Models;

namespace Nananet.Adapter.Fanbook.Api;

public abstract class BaseApi
{
    protected RestHandler RestHandler { get; private set; }
    protected ClientRuntimeData RuntimeData => RestHandler.RuntimeData;

    protected BaseApi(RestHandler restHandler)
    {
        RestHandler = restHandler;
    }
}