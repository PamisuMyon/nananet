using Nado.Core.Models;
using Nado.Core.Storage;

namespace Nado.Core.Utils;

public class Defender
{
    protected Spam _spam;
    protected IStorage _storage;

    public Defender(IStorage storage, int interval = 1000, int threshold = 10)
    {
        _storage = storage;
        _spam = new Spam(interval, threshold);
    }

    public async void Record(User user)
    {
        _spam.Record(user.UserId);
        if (!_spam.Check(user.UserId).Pass)
            await AddToBlockList(user);
    }

    public bool IsBlocked(string userId)
    {
        if (_storage.BlockList.Count == 0) return false;
        return _storage.BlockList.Any(u => u.UserId == userId);
    }

    public async Task AddToBlockList(User user)
    {
        _storage.BlockList.Add(user);
        await _storage.UpdateBlockList(_storage.BlockList);
        Logger.L.Info($"User added to block list: {user.UserId} {user.Name}");
    }

}