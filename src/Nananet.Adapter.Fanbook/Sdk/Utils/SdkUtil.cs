using System.Security.Cryptography;
using System.Text;

namespace Nananet.Adapter.Fanbook.Utils;

public static class SdkUtil
{
    
    public static string GetMd5(byte[] bytes)
    {
        using (var md5 = MD5.Create())
        {
            var hashBytes = md5.ComputeHash(bytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
    
    [Obsolete] // 不支持公钥解密
    public static string DecodeRsa(string encryptedText, string publicKeyPem)
    {
        var publicKeyPemTrimmed = publicKeyPem.Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "");
        var publicKeyBytes = Convert.FromBase64String(publicKeyPemTrimmed);
        
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, false);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
    
}