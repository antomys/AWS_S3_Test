using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using AWS_S3_Test.Models;
using AWS_S3_Test.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;

namespace AWS_S3_Test.Services
{
    public class S3Service : IS3Service
    {
        
        // I am little upset because of missing DRY here
        private readonly IAmazonS3 _amazonS3;
        private readonly ILogger<S3Service> _logger;
        private static readonly string DocPath 
            = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public S3Service(IAmazonS3 amazonS3, ILogger<S3Service> logger)
        {
            _amazonS3 = amazonS3;
            _logger = logger;
        }

        public async Task<AwsS3Response> CreateBucketAsync(string bucketName)
        {
            try
            {
                if (!await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName))
                {
                    var bucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    var response = await _amazonS3.PutBucketAsync(bucketRequest);

                    _logger.LogInformation("Successfully created bucket!");
                    return new AwsS3Response
                    {
                        RequestId = response.ResponseMetadata.RequestId,
                        StatusCode = (int) response.HttpStatusCode
                    };
                }

                throw new Exception("This bucket already exists");
            }
            //This is to catch amazon server exceptions
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
            catch (Exception)
            {
                _logger.LogError("Unknown exception");
                throw;
            }
        }
        public async Task<AwsS3Response> AddObjectToBucketAsync(string bucketName, 
            object randomJson, string newFileName)
        {
            if (randomJson == null)
                throw new InvalidDataException();
            //var pathToFile = await WriteObjectToFileAsync(randomJson);
            try
            {
                var streamJson = await SerializeToStream(randomJson);
                
                using var transferUtility = new TransferUtility(_amazonS3);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    InputStream = streamJson,
                    Key = Guid.NewGuid()
                        .ToString(),
                };


                if (newFileName != null)
                {
                    fileTransferUtilityRequest.Key = newFileName;
                }

                await transferUtility.UploadAsync(fileTransferUtilityRequest);
                
                _logger.LogInformation("Successfully pushed object to bucket");
                return new AwsS3Response
                {
                    StatusCode = 200,
                    RequestId = Path.GetFileName(fileTransferUtilityRequest.Key)
                };
            }
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unknown exception");
                throw;
            }
        }
        public async Task<string> GetBucketLocationAsync(string bucketName)
        {
            try
            {
                var location = await _amazonS3.GetBucketLocationAsync(
                    new GetBucketLocationRequest {BucketName = bucketName});
                _logger.LogInformation("Successfully retrieved bucket location!");
                return location.Location;
            }catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                return exception.Message;
            }
           
        }
        public async Task DeleteFromBucketAsync(string bucketName, string objectName)
        {
            try
            {
                await _amazonS3.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                });
                _logger.LogInformation("Successfully removed object from bucket!");
            }
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
        }

        public async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                await _amazonS3.DeleteBucketAsync(new DeleteBucketRequest {BucketName = bucketName});
                _logger.LogInformation("Successfully deleted bucket!");
            } 
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
        }
        public async Task<List<string>> GetAllFileNamesFromBucketAsync(string bucketName)
        {
            try
            {
                var response =
                    await _amazonS3.ListObjectsV2Async(
                        new ListObjectsV2Request {BucketName = bucketName});
                _logger.LogInformation("Successfully listed files from bucket!");
                
                return response.S3Objects.Select(file => file.Key).ToList();
            }
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
        }
        public async Task<object> GetObjectFromBucketAsync(string bucketName, string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return null;
            try
            {
                var getObject = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName
                };
                
                //Dunno why rider does not tells about bucket name but ok
                using var response = await _amazonS3.GetObjectAsync(getObject);
                await using var responseStream = response.ResponseStream;
                using var reader = new StreamReader(responseStream);

                var title = response.Metadata["x-amz-meta-title"] 
                            ?? objectName;
                
                // If response file has no metadata with given header or just null,
                // I will return object name as title
                var type = response.Headers["Content-Type"];
                var body = await reader.ReadToEndAsync();

                _logger.LogInformation("Successfully retrieved one obj from bucket!");
                return new
                {
                    Title = title,
                    Type = type,
                    Body = body
                };
            }
            catch (AmazonS3Exception exception)
            {
                _logger.LogError(exception, "Amazon server exception");
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unknown for me exception");
            }

            throw new InvalidOperationException();
        }
        
        private async Task<MemoryStream> SerializeToStream(object obj)
        {
            return await Task.Run(() =>
            {
                var jsonString = JsonConvert.SerializeObject(obj);
                return new MemoryStream(Encoding.Default.GetBytes(jsonString));
            });
        }
    }
}