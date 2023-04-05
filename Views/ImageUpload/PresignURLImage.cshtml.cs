using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Views.ImageUpload
{
    public class PresignURLImage : PageModel
    {
        private readonly ILogger<PresignURLImage> _logger;

        public PresignURLImage(ILogger<PresignURLImage> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}