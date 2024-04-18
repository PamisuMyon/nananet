using Newtonsoft.Json;

namespace Nananet.Adapter.Fanbook.Sdk.Models;

public class CosTempKey
{
    [JsonProperty("app_id")]
    public string AppId { get; set; }

    [JsonProperty("bucket")]
    public string Bucket { get; set; }

    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("host")]
    public string Host { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("secretId")]
    public string SecretId { get; set; }

    [JsonProperty("secretKey")]
    public string SecretKey { get; set; }

    [JsonProperty("audit_app_id")]
    public string AuditAppId { get; set; }

    [JsonProperty("audit_bucket")]
    public string AuditBucket { get; set; }

    [JsonProperty("audit_token")]
    public string AuditToken { get; set; }

    [JsonProperty("audit_host")]
    public string AuditHost { get; set; }

    [JsonProperty("audit_url")]
    public string AuditUrl { get; set; }

    [JsonProperty("audit_secretId")]
    public string AuditSecretId { get; set; }

    [JsonProperty("audit_type")]
    public string AuditType { get; set; }

    [JsonProperty("audit_secretKey")]
    public string AuditSecretKey { get; set; }

    [JsonProperty("upload_path")]
    public string UploadPath { get; set; }

    [JsonProperty("upload_path_service")]
    public string UploadPathService { get; set; }

    [JsonProperty("expired_time")]
    public int ExpiredTime { get; set; }

    [JsonProperty("start_time")]
    public int StartTime { get; set; }
}