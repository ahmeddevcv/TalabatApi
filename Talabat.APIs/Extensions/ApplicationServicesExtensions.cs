using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core;
using Talabat.Core.Repositories;
using Talabat.Core.Services;
using Talabat.Repository;
using Talabat.Service;

namespace Talabat.APIs.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IResponseCacheService, ResponseCacheService>();//12 Caching
            services.AddScoped<IPaymentService, PaymentService>();//10 CreateEndpoint
            services.AddScoped<IOrderService, OrderService>();//10 CreateEndpoint
            services.AddScoped<IUnitOfWork, UnitOfWork>();//8 allow di UnitOfWork
            //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));//8 allow di UnitOfWork
            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            //services.AddAutoMapper(m => m.AddProfile(new MappingProfiles()));
            services.AddAutoMapper(typeof(ProductToReturnDto)); //inject mapping

            ///ApiValidationErrorResponse
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count > 0)//ListOfParameters[errors]
                                                       .SelectMany(p => p.Value.Errors)//Errors in parameters
                                                       .Select(e => e.ErrorMessage)
                                                       .ToArray();
                    var validationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });
            return services;
        }
    }
}
