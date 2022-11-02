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

    public static string PathFromBase(string relativePath)
    {
        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    public static bool DeleteUnreliably(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
}