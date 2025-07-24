using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Shared;
using System;
using System.IO;
using System.Web;

namespace PraiseCMS.DataAccess.Helpers
{
    public static class AwsHelpers
    {
        public static bool UploadImage(string fileName, HttpPostedFileBase image, string path)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(ApplicationCache.Instance.AmazonConfiguration.AccessKey, ApplicationCache.Instance.AmazonConfiguration.SecretKey, RegionEndpoint.USEast1))
                {
                    var originalStream = new MemoryStream();
                    image.InputStream.Position = 0;
                    image.InputStream.CopyTo(originalStream);

                    var squareStream = new MemoryStream();
                    image.InputStream.Position = 0;
                    image.InputStream.CopyTo(squareStream);

                    var squareThumbStream = new MemoryStream();
                    image.InputStream.Position = 0;
                    image.InputStream.CopyTo(squareThumbStream);

                    var request = new PutObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, CannedACL = S3CannedACL.PublicRead, Key = path + "/" + fileName, InputStream = originalStream };
                    client.PutObject(request);

                    request.BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName;
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.Key = path + "/" + ApplicationCache.Instance.AmazonConfiguration.IconDirectory + "/" + fileName;
                    request.InputStream = Utilities.SquareImage(squareStream, 100, 100);

                    client.PutObject(request);

                    request.BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName;
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.Key = path + "/" + ApplicationCache.Instance.AmazonConfiguration.ThumbDirectory + "/" + fileName;
                    request.InputStream = Utilities.SquareAndScaleImage(squareThumbStream);

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool UploadFile(string fileName, HttpPostedFileBase file)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(ApplicationCache.Instance.AmazonConfiguration.AccessKey, ApplicationCache.Instance.AmazonConfiguration.SecretKey, RegionEndpoint.USEast1))
                {
                    var request = new PutObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, CannedACL = S3CannedACL.PublicRead, Key = fileName, InputStream = file.InputStream };

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool UploadFile(string fileName, Stream file)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(ApplicationCache.Instance.AmazonConfiguration.AccessKey, ApplicationCache.Instance.AmazonConfiguration.SecretKey, RegionEndpoint.USEast1))
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName,
                        CannedACL = S3CannedACL.PublicRead,
                        Key = fileName,
                        InputStream = file
                    };

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return false;
            }

            return true;
        }

        public static bool DeleteFile(string fileName)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(ApplicationCache.Instance.AmazonConfiguration.AccessKey, ApplicationCache.Instance.AmazonConfiguration.SecretKey, RegionEndpoint.USEast1))
                {
                    var request = new DeleteObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, Key = fileName };

                    client.DeleteObject(request);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return false;
        }

        public static bool DeleteImage(string fileName, string path)
        {
            try
            {
                using (var client = AWSClientFactory.CreateAmazonS3Client(ApplicationCache.Instance.AmazonConfiguration.AccessKey, ApplicationCache.Instance.AmazonConfiguration.SecretKey, RegionEndpoint.USEast1))
                {
                    var request = new DeleteObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, Key = path + "/" + fileName };

                    client.DeleteObject(request);

                    request = new DeleteObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, Key = path + "/" + ApplicationCache.Instance.AmazonConfiguration.IconDirectory + "/" + fileName };

                    client.DeleteObject(request);

                    request = new DeleteObjectRequest() { BucketName = ApplicationCache.Instance.AmazonConfiguration.BucketName, Key = path + "/" + ApplicationCache.Instance.AmazonConfiguration.ThumbDirectory + "/" + fileName };

                    client.DeleteObject(request);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            return false;
        }

        public static string AmazonLink(string fileName, string directory = null)
        {
            var path = !string.IsNullOrEmpty(directory) ? directory + "/" + fileName : fileName;
            var url = string.Format(ApplicationCache.Instance.AmazonConfiguration.PathTemplate.Replace("{bucket}", ApplicationCache.Instance.AmazonConfiguration.BucketName));
            return string.Concat(url, path).Replace("%20", "%2520");
        }
    }
}