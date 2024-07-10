using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DevExpress.ExpressApp.WebApi.Services;
using Microsoft.AspNetCore.OData;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;

namespace DXMvcCore.WebApi;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services) {

        services.AddXafWebApi(builder => {
            builder.ConfigureOptions(options => {
                // Make your business objects available in the Web API and generate the GET, POST, PUT, and DELETE HTTP methods for it.
                // options.BusinessObject<YourBusinessObject>();
            });

            builder.Modules
                .AddReports(options => {
                    options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.EF.ReportDataV2);
                })
                .AddValidation()
                .Add<DXMvcCore.WebApi.DXMvcCoreModule>();

            builder.ObjectSpaceProviders
                .AddEFCore(options => options.PreFetchReferenceProperties())
                    .WithAuditedDbContext(contexts => {
                        contexts.Configure<DXMvcCore.WebApi.BusinessObjects.DXMvcCoreEFCoreDbContext, DXMvcCore.WebApi.BusinessObjects.DXMvcCoreAuditingDbContext>(
                            (serviceProvider, businessObjectDbContextOptions) => {
                                // Uncomment this code to use an in-memory database. This database is recreated each time the server starts. With the in-memory database, you don't need to make a migration when the data model is changed.
                                // Do not use this code in production environment to avoid data loss.
                                // We recommend that you refer to the following help topic before you use an in-memory database: https://docs.microsoft.com/en-us/ef/core/testing/in-memory
                                //businessObjectDbContextOptions.UseInMemoryDatabase("InMemory");
                                string connectionString = null;
                                if(Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
                                ArgumentNullException.ThrowIfNull(connectionString);
                                businessObjectDbContextOptions.UseSqlServer(connectionString);
                                businessObjectDbContextOptions.UseChangeTrackingProxies();
                                businessObjectDbContextOptions.UseObjectSpaceLinkProxies();
                                businessObjectDbContextOptions.UseLazyLoadingProxies();
                            },
                            (serviceProvider, auditHistoryDbContextOptions) => {
                                string connectionString = null;
                                if(Configuration.GetConnectionString("ConnectionString") != null) {
                                    connectionString = Configuration.GetConnectionString("ConnectionString");
                                }
                                ArgumentNullException.ThrowIfNull(connectionString);
                                auditHistoryDbContextOptions.UseSqlServer(connectionString);
                                auditHistoryDbContextOptions.UseChangeTrackingProxies();
                                auditHistoryDbContextOptions.UseObjectSpaceLinkProxies();
                                auditHistoryDbContextOptions.UseLazyLoadingProxies();
                            },
                            auditTrailOptions => {
                                auditTrailOptions.AuditUserProviderType = typeof(DevExpress.Persistent.BaseImpl.EFCore.AuditTrail.AuditEmptyUserProvider);
                            });
                    })
                .AddNonPersistent();

            builder.AddBuildStep(application => {
                application.ApplicationName = "SetupApplication.DXMvcCore";
                application.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
#if DEBUG
                if(System.Diagnostics.Debugger.IsAttached && application.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                    application.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
                    application.DatabaseVersionMismatch += (s, e) => {
                        e.Updater.Update();
                        e.Handled = true;
                    };
                }
#endif
            });
        }, Configuration);

        services
            .AddControllers()
            .AddOData((options, serviceProvider) => {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c => {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "DXMvcCore API",
                Version = "v1",
                Description = @"Use AddXafWebApi(options) in the DXMvcCore.WebApi\Startup.cs file to make Business Objects available in the Web API."
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => {
            //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
            //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
            //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
            //Comment this code out to revert to the default behavior.
            //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if(env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DXMvcCore WebApi v1");
            });
        }
        else {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapXafEndpoints();
        });
    }
}
