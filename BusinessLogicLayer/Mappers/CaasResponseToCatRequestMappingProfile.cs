using AutoMapper;
using DataAccessLayer.Entities;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class CaasResponseToCatRequestMappingProfile : Profile
{
  public CaasResponseToCatRequestMappingProfile()
  {
    CreateMap<CaasResponse, CatRequest>()
      .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
      .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
      .ForMember(dest => dest.Image, opt => opt.Ignore())
      .ForMember(dest => dest.Created, opt => opt.Ignore())
      ;
  }
}
