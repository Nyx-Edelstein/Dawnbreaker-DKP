using System;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Data.Utility;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Utilities.UserData;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Dawnbreaker_DKP.Web.Utilities.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dawnbreaker_DKP
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAccountActionExecutor>(provider =>
            {
                var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
                var userLoginDataRepository = RepositoryFactory<UserLoginData>.UserDataRepository();
                var userAuthDataRepository = RepositoryFactory<UserAuthData>.UserDataRepository();
                var userPermissionsRepository = RepositoryFactory<UserPermissions>.UserDataRepository();
                return new AccountActionExecutor(httpContextAccessor, userLoginDataRepository, userAuthDataRepository, userPermissionsRepository);
            });
            services.AddTransient<IAccountRecovery>(provider =>
            {
                var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
                var userLoginDataRepository = RepositoryFactory<UserLoginData>.UserDataRepository();
                var userAuthDataRepository = RepositoryFactory<UserAuthData>.UserDataRepository();
                var userRecoveryTokenRepository = RepositoryFactory<UserRecoveryToken>.UserDataRepository();
                var keystoreRepository = RepositoryFactory<KeystoreEntry>.KeystoreRepository();
                var emailProvider = new EmailProvider(keystoreRepository);
                return new AccountRecovery(httpContextAccessor, userLoginDataRepository, userAuthDataRepository, userRecoveryTokenRepository, emailProvider);
            });
            services.AddTransient<IUserPermissionLookup>(provider =>
            {
                var userLoginDataRepository = RepositoryFactory<UserLoginData>.UserDataRepository();
                var userPermissionsRepository = RepositoryFactory<UserPermissions>.UserDataRepository();
                return new UserPermissionLookup(userLoginDataRepository, userPermissionsRepository);
            });
            services.AddTransient<IUserIdentityAuthenticator>(provider =>
            {
                var accountActionExecutor = provider.GetService<IAccountActionExecutor>();
                var userPermissionLookup = provider.GetService<IUserPermissionLookup>();
                return new UserIdentityAuthenticator(accountActionExecutor, userPermissionLookup);
            });
            services.AddTransient<IRaidSessionsUtil>(provider =>
            {
                var raidSessionRepository = RepositoryFactory<RaidSession>.DKPDataRepository();
                var sessionParticipantRepository = RepositoryFactory<SessionParticipant>.DKPDataRepository();
                var playerRecordRepository = RepositoryFactory<PlayerRecord>.DKPDataRepository();
                var ledgerRepository = RepositoryFactory<DKPLedgerEntry>.DKPDataRepository();
                return new RaidSessionsUtil(raidSessionRepository, sessionParticipantRepository, playerRecordRepository, ledgerRepository);
            });
            services.AddTransient<IRaidInitParser>(provider =>
            {
                return new RaidInitParser();
            });
            services.AddTransient<IDKPEntryParser>(provider =>
            {
                return new DKPEntryParser();
            });
            services.AddTransient<IPlayerManagementUtil>(provider =>
            {
                var playerRecordRepository = RepositoryFactory<PlayerRecord>.DKPDataRepository();
                var ledgerRepository = RepositoryFactory<DKPLedgerEntry>.DKPDataRepository();
                var raidSessionRepository = RepositoryFactory<RaidSession>.DKPDataRepository();
                var sessionParticipantRepository = RepositoryFactory<SessionParticipant>.DKPDataRepository();
                return new PlayerManagementUtil(playerRecordRepository, ledgerRepository, raidSessionRepository, sessionParticipantRepository);
            });
            services.AddTransient<IAuditUtil>(provider =>
            {
                var playerRecordRepository = RepositoryFactory<PlayerRecord>.DKPDataRepository();
                var ledgerRepository = RepositoryFactory<DKPLedgerEntry>.DKPDataRepository();
                var raidSessionRepository = RepositoryFactory<RaidSession>.DKPDataRepository();
                var sessionParticipantRepository = RepositoryFactory<SessionParticipant>.DKPDataRepository();
                return new AuditUtil(playerRecordRepository, ledgerRepository, raidSessionRepository, sessionParticipantRepository);
            });
            services.AddTransient<IImportUtil>(provider =>
            {
                var playerRecordRepository = RepositoryFactory<PlayerRecord>.DKPDataRepository();
                return new ImportUtil(playerRecordRepository);
            });
            services.AddTransient<IClassListUtil>(provider =>
            {
                var playerRecordRepository = RepositoryFactory<PlayerRecord>.DKPDataRepository();
                var classListRepository = RepositoryFactory<ClassListEntry>.DKPDataRepository();
                var raidSessionRepository = RepositoryFactory<RaidSession>.DKPDataRepository();
                var sessionParticipantRepository = RepositoryFactory<SessionParticipant>.DKPDataRepository();
                return new ClassListUtil(playerRecordRepository, classListRepository, raidSessionRepository, sessionParticipantRepository);
            });

            services.AddMvc();

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(186);
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 5001;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy => policy.RequireRole(PermissionsLevel.Admin.ToString()));
                options.AddPolicy("IsOfficer", policy => policy.RequireRole(PermissionsLevel.Officer.ToString()));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.None,
                Secure = CookieSecurePolicy.Always
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'none'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' https://wow.zamimg.com/ data:; connect-src 'self'; font-src 'self'; object-src 'self'; media-src 'self'; child-src 'self'; form-action 'self'; frame-ancestors 'none'; base-uri 'self'");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-Xss-Protection", "1");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                await next();
            });

            app.UseStatusCodePages();

            app.Use(serviceProvider.GetService<IUserIdentityAuthenticator>().GetUserIdentity);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("Server");
                await next();
            });
        }
    }
}
