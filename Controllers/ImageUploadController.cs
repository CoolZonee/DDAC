using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon; //link to aws account
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration; // appsettings.json
using System.IO;
using Microsoft.AspNetCore.Http;//ftp
using System.Net.Mime;

namespace WebApplication3.Controllers
{
    public class ImageUploadController : Controller
    {
        private readonly ILogger<ImageUploadController> _logger;

        private const string s3BucketName = "flower-tp055698";

        //function 1: connection string to the AWS Account
        private List<string> getValues()
        {
            List<string> values = new List<string>();

            //1. link to appsettings.json and get back the values
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); //build the json file

            //2. read the info from json using configure instance
            values.Add(configure["Values:Key1"]);
            values.Add(configure["Values:Key2"]);
            values.Add(configure["Values:Key3"]);

            return values;
        }

        //function 2: Upload image to S3 and generate the URL and store to DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessUploadImage(List<IFormFile> imagefile)
        {
            //1. add credential for action
            List<string> values = getValues();
            var awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            //function 2: Upload image to S3 and generate the URL and store to DB
            foreach (var image in imagefile)
            {
                if (image.Length <= 0)
                {
                    return BadRequest("It is an empty file. Unable to upload!");
                }

                else if (image.Length > 1048576) //not more than 1MB
                {
                    return BadRequest("It is over 1MB limit of size. Unable to upload!");
                }
                else if (image.ContentType.ToLower() != "image/png" && image.ContentType.ToLower() != "image/jpeg"
                    && image.ContentType.ToLower() != "image/gif")
                {
                    return BadRequest("It is not a valid image! Unable to upload!");
                }

                //3. upload image to S3 and get the URL
                Console.WriteLine("Uploading image to S3...");
                Console.WriteLine("Image name: " + image.FileName);
                try
                {
                    //upload to S3
                    PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                    {
                        InputStream = image.OpenReadStream(),
                        BucketName = s3BucketName + "/images",
                        Key = image.FileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    //send out the request
                    await awsS3client.PutObjectAsync(uploadRequest);
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);
                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);
                }
            }
            //4. return to Upload page again
            return RedirectToAction("Index", "ImageUpload");
        }


        public ImageUploadController(ILogger<ImageUploadController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        //function 3: Display image from S3 as gallery
        public async Task<IActionResult> DisplayImageFromS3()
        {
            //1. add credential for action
            List<string> values = getValues();
            var awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);


            List<S3Object> images = new List<S3Object>();

            try
            {
                // s3 token - telling whether still image in the S3 bucket
                string token = null;
                do
                {
                    //create List object request to the S3
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = s3BucketName
                    };

                    //getting response (images) back from the S3
                    ListObjectsResponse response = await awsS3client.ListObjectsAsync(request).ConfigureAwait(true);
                    images.AddRange(response.S3Objects);
                    token = response.NextMarker;
                } while (token != null);
                return View(images);
            }

            catch (AmazonS3Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        //function 4: Delete image from S3
        public async Task<IActionResult> DeleteImage(string ImageKey)
        {
            //1. add credential for action
            List<string> values = getValues();
            var awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            try
            {
                //create a delete request 
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = ImageKey
                };

                await awsS3client.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return RedirectToAction("DisplayImageFromS3", "ImageUpload");
        }

        public async Task<IActionResult> DownloadImage(string ImageKey)
        {
            List<string> values = getValues();
            var awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            Stream downloadStream = null;

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = ImageKey
                };

                GetObjectResponse response = await awsS3client.GetObjectAsync(request);

                using (downloadStream = response.ResponseStream)
                {
                    downloadStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(downloadStream);
                    downloadStream.Position = 0; // copy start from position 0 until the end
                }

            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest(ex.Message);
            }

            // make option either direct view in PC / direct download to PC
            string imageFile = Path.GetFileName(ImageKey);

            Response.Headers.Add(
                "Content-Disposition", new ContentDisposition
                {
                    FileName = imageFile,
                    Inline = false // true means direct view in PC, false means direct download to PC
                }.ToString());

            return File(downloadStream, "image/jpeg");
        }
    }



}