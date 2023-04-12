using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebApplication3.Controllers
{
    public class SNSController : Controller
    {
        private readonly ILogger<SNSController> _logger;

        public SNSController(ILogger<SNSController> logger)
        {
            _logger = logger;
        }

        private const string topicARN = "arn:aws:sns:us-east-1:009530277310:EmailBroadcast";

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

        public IActionResult Index()
        {
            ViewBag.msg = Request.Query["msg"];
            return View();
        }

        public IActionResult Error()
        {
            return View("Error!");
        }

        // function 2: user for email subscription in the SNS Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailSubscription(string email)
        {
            // connection
            List<string> values = getValues();
            var snsClient = new AmazonSimpleNotificationServiceClient(values[0], values[1], values[2], RegionEndpoint.USEast1);
            string subscriptionID = "";

            try
            {
                SubscribeRequest request = new SubscribeRequest(
                    topicARN,
                    "email",
                    email
                );
                SubscribeResponse response = await snsClient.SubscribeAsync(request);
                subscriptionID = response.ResponseMetadata.RequestId;
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                throw new Exception(ex.Message);
            }

            return RedirectToAction("Index", "SNS",
            new
            {
                msg = "Subscription Done! Your subscription ID = " + subscriptionID + ". Please check your email for confirmation!"
            }
                );
        }

        // function 3: create broatcast message
        public IActionResult CreateBroadcastMessage()
        {
            return View();
        }

        // function 4: process to broatcast message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastMessage(string message)
        {
            // connection
            List<string> values = getValues();
            var snsClient = new AmazonSimpleNotificationServiceClient(values[0], values[1], values[2], RegionEndpoint.USEast1);

            try
            {
                PublishRequest request = new PublishRequest(
                    topicARN,
                    message
                );
                PublishResponse response = await snsClient.PublishAsync(request);
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                throw new Exception(ex.Message);
            }

            return RedirectToAction("Index", "SNS",
            new
            {
                msg = "Message Broadcasted!"
            }
                );
        }
    }
}