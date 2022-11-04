using Nananet.Core.Models;
using Nananet.Core.Storage;

namespace Nananet.Core;

public interface IBot
{
    public IStorage Storage { get; }

    public BotConfig Config { get; }
    
    public Task Launch();

    public Task Refresh();

    public Task<string?> SendTextMessage(string targetId, string content, bool isPersonal,
        string? referenceId = null);

    public Task<string?> ReplyTextMessage(Message to, string content);

    public Task<string?> SendPictureFileMessage(string targetId, string filePath, bool isPersonal,
        string? referenceId = null);

    public Task<string?> ReplyPictureFileMessage(Message to, string filePath);

    public Task<string?> SendPictureUrlMessage(string targetId, string url, bool isPersonal,
        string? referenceId = null);

    public Task<string?> ReplyPictureUrlMessage(Message to, string url);

    public Task<bool> DeleteMessage(string messageId);

}