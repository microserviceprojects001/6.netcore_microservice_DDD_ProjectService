using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.Domain.Enums;
using AutoMapper;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 领域模型 -> DTO
            CreateMap<Order, OrderDto>()
                // 枚举转换成字符串
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));

            // DTO -> 领域模型
            CreateMap<OrderDto, Order>()
                // 字符串转枚举 (更新时可能用到，创建时用Create工厂方法)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OrderStatus>(src.Status)));

            CreateMap<OrderItemDto, OrderItem>()
                .ConstructUsing(dto => new OrderItem(dto.ProductId, dto.Quantity, dto.UnitPrice));
        }
    }
}
