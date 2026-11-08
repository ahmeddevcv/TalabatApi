namespace Talabat.APIs.Extensions
{
    public static class SwaggerServicesExtensions
    {
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer(); //// allow DI for (Swagger services)
            services.AddSwaggerGen();
            return services;
        }
        public static WebApplication UseSwaggerMiddlewares(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Talabat.APIs v1");
                options.RoutePrefix = string.Empty; // Serve Swagger UI at application root (/)
            });
            return app;
        }
    }
}
