using System.Security.Cryptography;
using System.Text;

public class CloudFrontService
{
    private readonly string _distributionDomain;
    private readonly string _keyPairId;
    private readonly string _privateKey;

    public CloudFrontService(IConfiguration config)
    {
        _distributionDomain = config["CloudFront:Domain"];
        _keyPairId = config["CloudFront:KeyPairId"];
        _privateKey = config["CloudFront:PrivateKey"];
    }

    public string GenerateSignedUrl(string fileKey, int expireMinutes = 60)
    {
        var url = $"https://{_distributionDomain}/{fileKey}";

        var expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes).ToUnixTimeSeconds();

        string policy = $"{{\"Statement\":[{{\"Resource\":\"{url}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expires}}}}}}}]}}";

        byte[] policyBytes = Encoding.UTF8.GetBytes(policy);

        string signature;
        using (var rsa = RSA.Create())
        {
            var privateKey = _privateKey
                .Replace("\\n", "\n")
                .Replace("\r", "");

            Console.WriteLine(_privateKey.Substring(0, 40));

            rsa.ImportFromPem(privateKey.ToCharArray());

            var signedBytes = rsa.SignData(policyBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            signature = ToUrlSafeBase64(signedBytes);
        }

        string encodedPolicy = ToUrlSafeBase64(policyBytes);

        return $"{url}?Policy={encodedPolicy}&Signature={signature}&Key-Pair-Id={_keyPairId}";
    }

    private string ToUrlSafeBase64(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("=", "_")
            .Replace("/", "~");
    }
}