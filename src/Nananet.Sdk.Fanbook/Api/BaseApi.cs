using Nananet.Sdk.Fanbook.Models;

namespace Nananet.Sdk.Fanbook.Api;

public abstract class BaseApi
{
    protected RestHandler RestHandler { get; private set; }
    protected ClientRuntimeData RuntimeData => RestHandler.RuntimeData;

    protected BaseApi(RestHandler restHandler)
    {
        RestHandler = restHandler;
    }
}