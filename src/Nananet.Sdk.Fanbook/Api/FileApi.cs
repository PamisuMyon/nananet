using System.Text;
using System.Text.Json.Nodes;
using COSXML;
using COSXML.Auth;
using COSXML.Transfer;
using Nananet.Sdk.Fanbook.Models;
using Nananet.Sdk.Fanbook.Models.Results;
using Nananet.Sdk.Fanbook.Utils;
using Nananet.Core.Utils;

namespace Nananet.Sdk.Fanbook.Api;

public class FileApi : BaseApi
{
    private CosTempKey? _cosTempKey;
    private CosXmlServer? _cosXmlServer;
    
    public FileApi(RestHandler restHandler) : base(restHandler)
    {
    }

    private async Task<CosTempKey?> GetCosTempKeyAsync()
    {
        const string url = "file/cosTmpKey";
        var result = await RestHandler.PostAsync<CommonResult<CosTempKey>>(url, "");
        if (result?.Status == true)
        {
            var cosTempKey = result.Data;
            
            // SecretKey需要RSA公钥解密，.NET中不太好实现，时间关系先RPC
            const string RsaUrl = "http://127.0.0.1:7006/rsa/decryptPublic";
            // var jo = new JsonObject();
            // jo["content"] = cosTempKey!.SecretKey;
            // var content = new StringContent(jo.ToString());
            var json = $@"{{ ""content"": ""{cosTempKey!.SecretKey}"" }}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            try
            {
                var response = await client.PostAsync(RsaUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    cosTempKey.SecretKey = responseBody;
                }
                else
                {
                    Logger.L.Error("RSA decrypt public RPC failed.");
                }
            }
            catch (Exception e)
            {
                Logger.L.Error("RSA decrypt public RPC error.");
                Logger.L.Error(e.StackTrace);
            }

            return cosTempKey;
        }

        return null;
    }

    public async Task<bool> SetupCosAsync()
    {
        if (_cosTempKey != null)
        {
            var beijingNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
            if (beijingNow.ToUnixTimeSeconds() < _cosTempKey.ExpiredTime)
            {
                if (_cosXmlServer != null)
                    return true;
            }
        }

        _cosTempKey = await GetCosTempKeyAsync();
        if (_cosTempKey == null)
        {
            Logger.L.Error("SetupCosAsync Get cos temp key failed.");
            return false;
        }
        
        var config = new CosXmlConfig.Builder()
            .SetRegion("ap-guangzhou")
            .SetDebugLog(true)
            .Build();
        // var durationSecond = 600;          // 每次请求签名有效时长，单位为秒
        // var qCloudCredentialProvider = new DefaultQCloudCredentialProvider(cosTempKey.SecretId, cosTempKey.SecretKey, durationSecond);
        var qCloudCredentialProvider = new DefaultSessionQCloudCredentialProvider(_cosTempKey.SecretId, _cosTempKey.SecretKey, _cosTempKey.StartTime, _cosTempKey.ExpiredTime, _cosTempKey.Token);
        _cosXmlServer = new CosXmlServer(config, qCloudCredentialProvider);
        
        return true;
    }

    public async Task<string?> UploadImageAsync(string filePath)
    {
        if (!await SetupCosAsync())
        {
            Logger.L.Error("UploadImageAsync Setup cos failed.");
            return null;
        }
        
        var cosPath = await GetImageCosPathAsync(_cosTempKey!, filePath);
        Console.WriteLine($"Cos path: {cosPath}");
        return await UploadFileAsync(_cosTempKey!, _cosXmlServer!, cosPath, filePath);
    }
    
    public async Task<string?> UploadFileAsync(CosTempKey cosTempKey, CosXmlServer cosXmlServer, string cosPath, string srcPath)
    {
        // 初始化 TransferConfig
        var transferConfig = new TransferConfig();
        // 手动设置开始分块上传的大小阈值为10MB，默认值为5MB
        transferConfig.DivisionForUpload = 10 * 1024 * 1024;
        // 手动设置分块上传中每个分块的大小为2MB，默认值为1MB
        transferConfig.SliceSizeForUpload = 2 * 1024 * 1024;

        // 初始化 TransferManager
        var transferManager = new TransferManager(_cosXmlServer, transferConfig);

        // 上传对象
        var uploadTask = new COSXMLUploadTask(cosTempKey.Bucket, cosPath);
        uploadTask.SetSrcPath(srcPath);

        uploadTask.progressCallback = delegate (long completed, long total)
        {
            Console.WriteLine("progress = {0:##.##}%", completed * 100.0 / total);
        };

        try {
            var result = await transferManager.UploadAsync(uploadTask);
            Console.WriteLine(result.GetResultInfo());
            if (result.IsSuccessful())
            {
                return $"{cosTempKey.Host}/{cosPath}";
            }
        } catch (Exception e) {
            Logger.L.Error("CosException: " + e);
            Logger.L.Error(e.StackTrace);
        }

        return null;
    }
    
    private static async Task<string> GetFileMd5Async(string filePath)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);
        return SdkUtil.GetMd5(bytes);
    }

    public static async Task<string> GetImageCosPathAsync(CosTempKey cosTempKey, string filePath)
    {
        var md5 = await GetFileMd5Async(filePath);
        var imageName = $"{md5}{Path.GetExtension(filePath)}";
        return $"{cosTempKey.UploadPath}image/{imageName}";
    }
    
    public static async Task<string> GetImageUrlAsync(CosTempKey cosTempKey, string filePath)
    {
        var md5 = await GetFileMd5Async(filePath);
        var imageName = $"{md5}{Path.GetExtension(filePath)}";
        return $"{cosTempKey.Host}/{cosTempKey.UploadPath}/image/{imageName}";
    }
    
}