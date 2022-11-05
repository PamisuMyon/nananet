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

    public Task<string?> SendLocalFileMessage(string targetId, string filePath, bool isPersonal,
        string? referenceId = null, FileType fileType = FileType.File);

    public Task<string?> ReplyLocalFileMessage(Message to, string filePath, FileType fileType = FileType.File);

    public Task<string?> SendServerFileMessage(string targetId, string url, bool isPersonal,
        string? referenceId = null, FileType fileType = FileType.File);

    public Task<string?> ReplyServerFileMessage(Message to, string url, FileType fileType = FileType.File);

    public Task<bool> DeleteMessage(string? targetId, string messageId);

}

public enum FileType
{
    File,
    Image,
    Video,
    Audio,
}
