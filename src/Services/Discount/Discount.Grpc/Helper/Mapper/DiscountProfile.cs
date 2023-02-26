using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;

namespace Discount.Grpc.Helper.Mapper
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<Coupon, CouponModel>().ReverseMap();
        }
    }
}
