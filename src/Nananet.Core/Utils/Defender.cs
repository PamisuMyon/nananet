using Nananet.Core.Models;
using Nananet.Core.Storage;

namespace Nananet.Core.Utils;

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
        _spam.Record(user.Id);
        if (!_spam.Check(user.Id).Pass)
            await AddToBlockList(user);
    }

    public bool IsBlocked(string userId)
    {
        if (_storage.BlockList.Count == 0) return false;
        return _storage.BlockList.Any(u => u.Id == userId);
    }

    public async Task AddToBlockList(User user)
    {
        _storage.BlockList.Add(user);
        await _storage.UpdateBlockList(_storage.BlockList);
        Logger.L.Info($"User added to block list: {user.Id} {user.NickName}");
    }

}