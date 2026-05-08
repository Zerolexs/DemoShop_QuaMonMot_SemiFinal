using AutoMapper;
using DemoShop_QuaMonMot.DTOs;
using DemoShop_QuaMonMot.Models;
namespace DemoShop_QuaMonMot.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<DangKy, KhachHang>();
        //CreateMap<Account, AccountDTO>().ReverseMap();
        //CreateMap<ContentType, M_SelectDropDown>()
        //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
        //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));
    }
}
