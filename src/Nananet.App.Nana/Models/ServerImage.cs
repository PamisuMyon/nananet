using MongoDB.Entities;
using Nananet.Core.Utils;

namespace Nananet.App.Nana.Models;

[Collection("server-images")]
public class ServerImage : Entity, ICreatedOn, IModifiedOn
{
    public string Platform { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public string Thumbnail { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ImageId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }

    public static async Task<ServerImage?> FindOne(string platform, string fileName)
    {
        return await DB.Find<ServerImage>()
            .Match(s => s.Platform == platform)
            .Match(s => s.FileName == fileName)
            .ExecuteFirstAsync();
    }

    public static async Task DeleteOne(string platform, string fileName)
    {
        await DB.DeleteAsync<ServerImage>(s => s.Platform == platform && s.FileName == fileName);
    }

    public static async Task<ServerImage?> GetOne(string platform, string fileName)
    {
        var serverImage = await FindOne(platform, fileName);
        if (serverImage != null)
        {
            var isAlive = await NetUtil.IsUrlAlive(serverImage.Url);
            if (isAlive)
                return serverImage;
            await serverImage.DeleteAsync();
        }
        return null;
    }

}
