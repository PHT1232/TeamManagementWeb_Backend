using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace TeamManagementProject_Backend.Helpers
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public EncryptionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            List<string> excludeURL = GetExcludeUrlList();
            if (!excludeURL.Contains(httpContext.Request.Path.Value))
            {
                httpContext.Request.Body = DecryptStream(httpContext.Request.Body);
                if (httpContext.Request.QueryString.HasValue)
                {
                    string decryptedString = DecryptString(httpContext.Request.QueryString.Value.Substring(1));
                    httpContext.Request.QueryString = new QueryString($"?{decryptedString}");
                }
            }
            await _next(httpContext);
        }

        private CryptoStream EncryptStream(Stream responseStream)
        {
            Aes aes = GetEncryptionAlgorithm();
            ToBase64Transform base64Transform = new ToBase64Transform();
            CryptoStream base64EncodedStream = new CryptoStream(responseStream, base64Transform, CryptoStreamMode.Write);
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(base64EncodedStream, encryptor, CryptoStreamMode.Write);

            return cryptoStream;
        }

        static byte[] Encrypt(string plainText)
        {
            byte[] encrypted;
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        encrypted = memoryStream.ToArray();
                    }
                }
            }

            return encrypted;
        }

        private Stream DecryptStream(Stream cipherStream)
        {
            Aes aes = GetEncryptionAlgorithm();
            FromBase64Transform base64Transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
            CryptoStream base64DecodedStream = new CryptoStream(cipherStream, base64Transform, CryptoStreamMode.Read);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream decryptedStream = new CryptoStream(base64DecodedStream, decryptor, CryptoStreamMode.Read);

            return decryptedStream;
        }

        private string DecryptString(string cipherText)
        {
            Aes aes = GetEncryptionAlgorithm();
            byte[] buffer = Convert.FromBase64String(cipherText);
            MemoryStream memoryStream = new MemoryStream(buffer);
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        private Aes GetEncryptionAlgorithm()
        {
            Aes aes = Aes.Create();
            var secret_key = Encoding.UTF8.GetBytes("1909510200190920");
            var initialization_vector = Encoding.UTF8.GetBytes("1909510200190920");
            aes.Key = secret_key;
            aes.IV = initialization_vector;

            return aes;
        }

        private List<string> GetExcludeUrlList()
        {
            List<string> excludeURL = new List<string>();
            excludeURL.Add("/api/chat/negotiate");
            excludeURL.Add("/api/file/GetUserProfile");
            excludeURL.Add("/api/chat");
            return excludeURL;
        }
    }
}
