using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddControllers();
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LineBot_Order", Version = "v1" });
            //});

            services.AddSingleton<LineBotConfig, LineBotConfig>((s) => new LineBotConfig
            {
                Channel_Secret = Configuration["LineBot:Channel_Secret"],
                Channel_access_token = Configuration["LineBot:Channel_access_token"]
            });

            services.AddSingleton<LineNotifyConfig, LineNotifyConfig>((s) => new LineNotifyConfig
            {
                ClientId = Configuration["LineNotify:ClientId"],
                ClientSecret = Configuration["LineNotify:ClientSecret"],
                CallbackUrl = Configuration["LineNotify:CallbackUrl"],
                AuthorizeUrl = Configuration["LineNotify:AuthorizeUrl"],
                TokenUrl = Configuration["LineNotify:TokenUrl"],
                State = Configuration["LineNotify:State"],
                SuccessUrl = Configuration["LineNotify:SuccessUrl"],
                NotifyUrl = Configuration["LineNotify:NotifyUrl"]
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // ³]©w Log ¸ô®|
            loggerFactory.AddFile("Logs/LineBot_Order-{Date}.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Login}/{id?}");
            });
        }
    }
}
