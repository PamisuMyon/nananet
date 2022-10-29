namespace Nado.Core.Utils;

public static class FileUtil
{

    public static async Task<T?> ReadJson<T>(string path, bool shouldLog = true)
    {
        if (!File.Exists(path))
        {
            if (shouldLog)
                Logger.L.Error($"{path} not exists.");
            return default;
        }
        var str = await File.ReadAllTextAsync(path);
        return !string.IsNullOrEmpty(str) ? JsonUtil.FromJson<T>(str) : default;
    }
    
    
}