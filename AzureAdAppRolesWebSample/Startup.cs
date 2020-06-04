using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureAdAppRolesWebSample
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AddAzureAdJwtAuth(services);
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void AddAzureAdJwtAuth(IServiceCollection services)
        {
            // TODO: update "AzureAd:Authority" and "AzureAd:ClientId" in appsetting.json.
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));

            services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.Authority =  Configuration.GetValue<string>("AzureAd:Authority");
                    jwtOptions.Audience = Configuration.GetValue<string>("AzureAd:ClientId");

                    if (_env.IsDevelopment())
                    {
                        // テキトーにトークンの validation の条件変更を書く(有効期限の検証無視はよくないヨ)
                        jwtOptions.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateLifetime = false,
                        };

                        // エラーイベントもかける（デバッグ専用って用途）
                        jwtOptions.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = AuthenticationFailed
                        };
                    }
                });
        }

        private static async Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            var message = $"AuthenticationFailed: {arg.Exception.Message}";
            arg.Response.ContentLength = message.Length;
            arg.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await arg.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length);
        }
    }
}