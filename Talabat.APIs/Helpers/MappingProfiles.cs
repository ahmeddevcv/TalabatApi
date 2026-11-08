using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.APIs.Helpers
{
    public class MappingProfiles:Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(x => x.ProductBrand, y => y.MapFrom(z => z.ProductBrand.Name))
                .ForMember(x => x.ProductType, y => y.MapFrom(z => z.ProductType.Name))
                .ForMember(d=>d.PictureUrl, o=>o.MapFrom<ProductPictureUrlResolver>());
            CreateMap<Talabat.Core.Entities.Identity.Address, AddressDto>().ReverseMap();//8 GetUserAddres

            CreateMap<CustmerBasketDto,CustmerBasket> ().ReverseMap();
            CreateMap<BasketItemDto, BasketItem>().ReverseMap();
            CreateMap<AddressDto, Talabat.Core.Entities.Order_Aggregate.Address>();
            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, o => o.MapFrom(s => s.DeliveryMethod.ShortName))//then add DeliveryMethod.cost
                .ForMember(d => d.DeliveryMethodCost, o => o.MapFrom(s => s.DeliveryMethod.Cost));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, o => o.MapFrom(s => s.Product.ProductId))
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.Product.PictureUrl))
                .ForMember(d => d.PictureUrl, o => o.MapFrom<OrderItemPictureUrlResolver>());





        }
    }
}
