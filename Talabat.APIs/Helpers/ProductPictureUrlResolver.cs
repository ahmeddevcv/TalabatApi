using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;

namespace Talabat.APIs.Helpers
{
    public class ProductPictureUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
    {
        private readonly IConfiguration _configuration;

        public ProductPictureUrlResolver(IConfiguration configuration)//inject IConfiguration
        {
            _configuration = configuration;
        }
        public string Resolve(Product source, ProductToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.PictureUrl))
            {
                // External images (pasted full URLs) must not get the API base URL prepended
                if (source.PictureUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    source.PictureUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return source.PictureUrl;

                return $"{_configuration["ApiBaseUrl"]}{source.PictureUrl}";
            }
            return string.Empty;
        }
    }
}
