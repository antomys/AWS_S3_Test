using System.Threading.Tasks;
using AWS_S3_Test.Models;
using AWS_S3_Test.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AWS_S3_Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BucketController: ControllerBase
    {

        private readonly IS3Service _s3Service;
        public BucketController(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateBucket(string bucketName = "test-bucket-pma")
        {
            var response = await _s3Service.CreateBucketAsync(bucketName.ToLower());
            if (response is null)
                return BadRequest();
            return Ok(response);
        }
        
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> AddObject(JsonObject jsonObject,
            string newFileName = null, string bucketName = "test-bucket-pma")
        {
            return Ok(await _s3Service
                .AddObjectToBucketAsync
                    (bucketName.ToLower(),jsonObject, newFileName));
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetBucketLocation(string bucketName = "test-bucket-pma")
        {
            return Ok(await _s3Service.GetBucketLocationAsync(bucketName));
        }
        
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetBucketFileNames(string bucketName = "test-bucket-pma")
        {
            return Ok(await _s3Service.GetAllFileNamesFromBucketAsync(bucketName));
        }
        
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetFileFromBucket(string fileName, string bucketName = "test-bucket-pma")
        {
            return Ok( await _s3Service.GetObjectFromBucketAsync(bucketName, fileName));
        }
        [HttpDelete]
        [Route("[action]")]
        public async Task<IActionResult> DeleteObjectFromBucket(string fileName, string bucketName = "test-bucket-pma")
        {
            return Ok( await _s3Service.DeleteFromBucketAsync(bucketName,fileName));
        }
        [HttpDelete]
        [Route("[action]")]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName = "test-bucket-pma")
        {
            return Ok( await _s3Service.DeleteBucketAsync(bucketName));
        }
            
    }
}