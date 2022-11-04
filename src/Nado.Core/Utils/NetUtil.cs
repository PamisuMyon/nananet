using RestSharp;

namespace Nado.Core.Utils;

public class NetUtil
{
    public static Task DownloadFile(string url, string dirPath, string fileName)
    {
        Directory.CreateDirectory(dirPath);
        
        // v107
        // var client = new RestClient(url);
        // await using var stream = await client.DownloadStreamAsync(new RestRequest());
        // if (stream != null)
        // {
        //     await using var output = File.Open(Path.Combine(dirPath, fileName), FileMode.Create);
        //     await stream.CopyToAsync(output);
        //     return true;
        // }

        // v106
        var client = new RestClient(url);
        return Task.Factory.StartNew(() =>
        {
            var bytes = client.DownloadData(new RestRequest());
            File.WriteAllBytes(Path.Combine(dirPath, fileName), bytes);
        });
        
    }

    public static async Task<bool> IsUrlAlive(string url, int timeout = 1000)
    {
        var client = new RestClient(url)
        {
            Timeout = timeout
        };
        var response = await client.ExecuteAsync(new RestRequest(Method.OPTIONS));
        return response.IsSuccessful;
    }
    
}