using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication3.Areas.Identity.Data;
using WebApplication3.Data;

[assembly: HostingStartup(typeof(WebApplication3.Areas.Identity.IdentityHostingStartup))]
namespace WebApplication3.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<WebApplication3Context>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("WebApplication3ContextConnection")));

                services.AddDefaultIdentity<WebApplication3User>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<WebApplication3Context>();
            });
        }
    }
}
