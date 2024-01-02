using kithub.api.models.PhonePe;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace kithub.api.Extensions
{
    public static class ProcessPaymentData
    {
        public static string ConvertToBase64(this Payload payload)
        {            
            var wncoded = JsonConvert.SerializeObject(payload);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(wncoded);
            var base64 = System.Convert.ToBase64String(plainTextBytes);

            return base64;
        }

        public static string GenerateHash(this string payload)
        {
            SHA256 sha256Hash = SHA256.Create();
            var xfiles = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(payload));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < xfiles.Length; i++)
            {
                builder.Append(xfiles[i].ToString("x2"));
            }
            builder.Append("###1");
            string xverify = builder.ToString();

            return xverify;
        }

    }
}
