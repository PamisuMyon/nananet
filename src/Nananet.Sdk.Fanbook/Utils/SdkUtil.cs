using System.Security.Cryptography;
using System.Text;

namespace Nananet.Sdk.Fanbook.Utils;

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
    
}