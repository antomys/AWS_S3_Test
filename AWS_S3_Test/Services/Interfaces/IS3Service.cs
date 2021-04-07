using System.Threading.Tasks;
using AWS_S3_Test.Models;

namespace AWS_S3_Test.Services.Interfaces
{
    public interface IS3Service
    {
        Task<AwsS3Response> CreateBucketAsync(string bucketName);
        Task<object> AddObjectToBucketAsync(string bucketName,
            object randomJson, string newFileName);
        Task<string> GetBucketLocationAsync(string bucketName);
        Task<object> GetObjectFromBucketAsync(string bucketName, string objectName);
        Task<object> GetAllFileNamesFromBucketAsync(string bucketName);
        Task<object> DeleteFromBucketAsync(string bucketName, string objectName);
        Task<object> DeleteBucketAsync(string bucketName);
    }
}