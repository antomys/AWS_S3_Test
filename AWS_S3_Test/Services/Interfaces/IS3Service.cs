using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AWS_S3_Test.Models;

namespace AWS_S3_Test.Services.Interfaces
{
    public interface IS3Service
    {
        Task<AwsS3Response> CreateBucketAsync(string bucketName);
        Task<AwsS3Response> AddObjectToBucketAsync(string bucketName,
            object randomJson, string newFileName);
        Task<string> GetBucketLocationAsync(string bucketName);
        Task<object> GetObjectFromBucketAsync(string bucketName, string objectName);
        Task<List<string>> GetAllFileNamesFromBucketAsync(string bucketName);
        Task DeleteFromBucketAsync(string bucketName, string objectName);
        Task DeleteBucketAsync(string bucketName);
    }
}