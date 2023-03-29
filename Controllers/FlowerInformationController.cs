using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace WebApplication3.Controllers
{
    public class FlowerInformationController : Controller
    {

        private const string s3BucketName = "flower-tp055698";

        //function extra: connection string to the AWS Account
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
        private readonly WebApplication3Context _context;

        public FlowerInformationController(WebApplication3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // read the data from the database to a list
            List<Flower> flowerList = await _context.Flower.ToListAsync();
            return View(flowerList);
        }

        public IActionResult AddData()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddData([Bind("FlowerName,FlowerProducedDate,FlowerType,FlowerPrice")] Flower flower, IFormFile imagefile)
        {
            try
            {
                List<string> getKeys = getValues();
                var awsS3client = new AmazonS3Client(getKeys[0], getKeys[1], getKeys[2], RegionEndpoint.USEast1);

                //upload to S3
                PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                {
                    InputStream = imagefile.OpenReadStream(),
                    BucketName = s3BucketName + "/images",
                    Key = imagefile.FileName,
                    CannedACL = S3CannedACL.PublicRead
                };

                await awsS3client.PutObjectAsync(uploadRequest);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            flower.FlowerURL = "https://" + s3BucketName + ".s3.amazonaws.com/images/" + imagefile.FileName;
            flower.FlowerS3Key = imagefile.FileName;
            if (ModelState.IsValid)
            {
                _context.Add(flower);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(flower);
        }

        // create a update page and load it
        public async Task<IActionResult> EditPage(int? FlowerID)
        {
            if (FlowerID == null)
            {
                return NotFound();
            }

            Flower flower = await _context.Flower.FindAsync(FlowerID);
            return View(flower);
        }

        public async Task<IActionResult> processEditFunction(Flower flower)
        {
            if (ModelState.IsValid)
            {
                ViewBag.message = "Update successfully!";
                _context.Flower.Update(flower);
                await _context.SaveChangesAsync();
                return RedirectToAction("index", "FlowerInformation");
            }
            ViewBag.message = "Failed to update!";
            return View("EditPage");
        }

        public async Task<IActionResult> DeletePage(int? flowerID)
        {
            if (flowerID == null)
            {
                return NotFound();
            }

            try
            {

                var flower = await _context.Flower.FindAsync(flowerID);
                if (flower != null)
                {
                    _context.Flower.Remove(flower);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("index", "FlowerInformation");
                }
                else
                {
                    return RedirectToAction("index", "FlowerInformation");

                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("index", "FlowerInformation");
            }
        }
    }
}
