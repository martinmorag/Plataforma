using Amazon.S3;
using Amazon.S3.Model;

namespace Plataforma.Servicios
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IAmazonS3 s3Client, IConfiguration config)
        {
            _s3Client = s3Client;
            _bucketName = config["AWS:BucketName"];
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var key = $"private/{folder}/{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            return key; 
        }
        public async Task DeleteFileAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}
