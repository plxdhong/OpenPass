using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Pass.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Pass.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using BotDetect.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

namespace Pass
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

            //����The JSON serializer is synchronously writing to the response body.
            //Ϊ��֤������AllowSynchronousIO
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(@"Data Source="+Configuration.GetConnectionString("DataDB")));
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://cfew.rainballs.com",
                                            "https://pass.rainballs.com")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod()
                                            .AllowCredentials();
                    });
            });
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            /// Email Sender DI
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<HtmlIo>();
            services.AddRazorPages();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(Config.GetApiScopes())
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.IdentityResources())
                .AddAspNetIdentity<IdentityUser>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole",
                     policy => policy.RequireRole("Admin"));
            });

            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Unspecified; //SameSiteMode.Unspecified in .NET Core 3.1
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Unspecified; //SameSiteMode.Unspecified in .NET Core 3.1
            });

            // Captcha
            // Add Session services.
            //Ϊ��֤��
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.IsEssential = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IServiceProvider service)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.Use((context, next) =>
            {
                if (!env.IsDevelopment())
                    context.Request.Scheme = "https";

                return next();
            });
            app.UseIdentityServer();
            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            // configure your application pipeline to use Captcha middleware
            // Important! UseCaptcha(...) must be called after the UseSession() call
            app.UseCaptcha(Configuration);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
            // ��ʼ��ADMIN��ɫ
            CreateUserRoles(service).Wait();
        }
        // ���������ڴ���ADMIN��ɫ��������
        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            IdentityResult roleResult;
            //Adding Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                Console.WriteLine("��ʼ��������Admin ROLE......");
                //create the roles and seed them to the database
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin"));
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("��ʼ��������Admin ROLE�ɹ�");
                }
                else
                {
                    throw new InvalidOperationException($"��ʼ��������Admin ROLEʧ��:{roleResult.ToString()}");
                }
            }
            else
            {
                Console.WriteLine("��ʼ�������贴��Admin ROLE......");
            }
            //Assign Admin role to the main User here we have given our newly registered
            //login id for Admin management
            IdentityUser user = await UserManager.FindByEmailAsync("xxxxxxxxxx@xxxx.com");
            if (user==null)
            {
                Console.WriteLine("��ʼ��������xxxxxxxx@xxxx.com......");
                var newUser = new IdentityUser { UserName = "xxxx@xxxx.com", Email = "xxxx@xxxx.com" };
                var result = await UserManager.CreateAsync(newUser, "**************");
                if (result.Succeeded)
                {
                    Console.WriteLine("��ʼ��������xxxx@xxxx.com�ɹ�");
                    user = await UserManager.FindByEmailAsync("xxxx@xxxx.com");
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = $"/Identity/Account/ConfirmEmail?userId={user.Id}&email={user.Email}&code={code}";
                    Console.WriteLine("��������");
                    Console.WriteLine(callbackUrl);
                }
                else
                {
                    throw new InvalidOperationException($"����xxxx@xxxx.comʧ��:{result.ToString()}");
                }
            }
            else
            {
                Console.WriteLine("��ʼ�������贴����ʼ�˺�......");
            }
            bool inRoleCheck = await UserManager.IsInRoleAsync(user, "Admin");
            if (!inRoleCheck)
            {
                Console.WriteLine("��ʼ������Admin ROLE......");
                roleResult = await UserManager.AddToRoleAsync(user, "Admin");
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("��ʼ������Admin ROLE�ɹ�");
                }
                else
                {
                    throw new InvalidOperationException($"��ʼ������Admin ROLEʧ��:{roleResult.ToString()}");
                }
            }
            else
            {
                Console.WriteLine("��ʼ���������Admin ROLE......");
            }
        }
    }

}
