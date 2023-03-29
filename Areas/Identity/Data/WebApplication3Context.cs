using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Areas.Identity.Data;

namespace WebApplication3.Data
{
    public class WebApplication3Context : IdentityDbContext<WebApplication3User>
    {
        public WebApplication3Context(DbContextOptions<WebApplication3Context> options)
            : base(options)
        {
        }

        public DbSet<WebApplication3.Models.Flower> Flower { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
