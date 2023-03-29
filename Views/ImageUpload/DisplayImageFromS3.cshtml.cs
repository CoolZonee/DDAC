using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Views.ImageUpload
{
    public class DisplayImageFromS3 : PageModel
    {
        private readonly ILogger<DisplayImageFromS3> _logger;

        public DisplayImageFromS3(ILogger<DisplayImageFromS3> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}