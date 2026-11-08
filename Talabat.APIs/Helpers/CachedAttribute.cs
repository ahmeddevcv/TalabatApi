using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Talabat.Core.Services;

namespace Talabat.APIs.Helpers
{
    public class CachedAttribute: Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;
        public CachedAttribute(int timeToLiveSeconds = 60)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            // Allow bypassing cache if requested (e.g. admin panels or when user forces fresh data)
            bool bypassCache = (request.Headers.TryGetValue("Cache-Control", out var cc) && cc.ToString().Contains("no-cache", StringComparison.OrdinalIgnoreCase))
                               || request.Headers.ContainsKey("Pragma")
                               || request.Query.ContainsKey("bypassCache");

            if (bypassCache)
            {
                await next();
                return;
            }

            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var cacheKey = GenerateCacheKeyFromRequest(request);
            var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var contentResult = new ContentResult
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }
            var executedContext = await next(); //move to the action
            if (executedContext.Result is OkObjectResult result)
            {
                await cacheService.CacheResponseAsync(cacheKey, result.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
            }
        }
        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
