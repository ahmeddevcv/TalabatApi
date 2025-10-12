using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.APIs.Helpers;
using Talabat.APIs.Middlewares;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repositories;
using Talabat.Repository;
using Talabat.Repository.Data;
using Talabat.Repository.Identity;

namespace Talabat.APIs
{
    public class Program
    {
        //with Eng Ahmed Nasr
        ////enttry point
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services
            // Add services to the container.

            builder.Services.AddControllers(); //// allow DI for (web_api services)



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            ///builder.Services.AddEndpointsApiExplorer(); //// allow DI for (Swagger services)
            ///builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerServices();

            builder.Services.AddDbContext<StoreContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("cs"));
            });
            //1 identity
            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(Options =>
            {
                var connection = builder.Configuration.GetConnectionString("redis");
                return ConnectionMultiplexer.Connect(connection);
        });

            #region Injection Will be added to extension method

            ////Register IGenericRepository<>
            //builder.Services.AddScoped<IGenericRepository<Product>, IGenericRepository<Product>>();
            //builder.Services.AddScoped<IGenericRepository<ProductType>, IGenericRepository<ProductType>>();
            //builder.Services.AddScoped<IGenericRepository<ProductBrand>, IGenericRepository<ProductBrand>>();
            //builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            ////builder.Services.AddAutoMapper(m => m.AddProfile(new MappingProfiles()));
            //builder.Services.AddAutoMapper(typeof(ProductToReturnDto)); //inject mapping

            ///ApiValidationErrorResponse
            ///builder.Services.Configure<ApiBehaviorOptions>(options =>
            ///{
            ///    options.InvalidModelStateResponseFactory = (actionContext) =>
            ///    {
            ///        var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count > 0)//ListOfParameters[errors]
            ///                                           .SelectMany(p => p.Value.Errors)//Errors in parameters
            ///                                           .Select(e => e.ErrorMessage)
            ///                                           .ToArray();
            ///        var validationErrorResponse = new ApiValidationErrorResponse()
            ///        {
            ///            Errors = errors
            ///        };
            ///        return new BadRequestObjectResult(validationErrorResponse);
            ///    };
            ///}); 
            #endregion
            builder.Services.AddApplicationServices();
            //builder.Services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            //2 identity_Seed
            //builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>();
            //builder.Services.AddAuthentication();
            //6 test auth
            builder.Services.AddIdentityServices(builder.Configuration);

            //angular lec6
            //allow di Cors Origin services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", options =>
                {
                    options.AllowAnyHeader().AllowAnyMethod().AllowAnyHeader().WithOrigins(builder.Configuration["FrontBaseUrl"]);
                });
            });
            #endregion

            var app = builder.Build();
            #region Applu Migration And Data Seeding
            /// automatic updata
            using var scope = app.Services.CreateScope(); //// explictly     using=>dispose for object
            var servies = scope.ServiceProvider;
            var loggerFactory = servies.GetRequiredService<ILoggerFactory>(); //ask clr to create obj....
            try
            {
                var dbContext = servies.GetRequiredService<StoreContext>();
                await dbContext.Database.MigrateAsync(); //update db
                await StoreContextDataSeed.SeedAsync(dbContext);

                //1 identity
                var identityDbContext = servies.GetRequiredService<AppIdentityDbContext>();
                await identityDbContext.Database.MigrateAsync();

                //2 identity_Seed
                var userManger = servies.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUsersAsync(userManger);
            }
            catch (Exception ex)
            {
                var log = loggerFactory.CreateLogger<Program>();
                log.LogError(ex, "Error occured during migartion");
            } 
            #endregion

            #region Configured app[kestral] middlewares
            app.UseMiddleware<ExceptionMiddleware>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {//// when i am in development, I use (2 Swagger middelwares)
                ///app.UseSwagger();
                ///app.UseSwaggerUI();
                app.UseSwaggerMiddlewares();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");//notfound endpoint

            app.UseHttpsRedirection(); //// middleware for https

            //app.UseAuthorization(); //// security

            app.UseStaticFiles();////images
            app.UseCors("MyPolicy");//angular lec6

            //6 test auth
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers(); //// routing, and excuting endopoints

            #endregion
            app.Run(); 
        }
    }
}