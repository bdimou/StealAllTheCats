using AutoMapper;
using DataAccessLayer.Entities;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class CatTagRequestToCatTagRequestMappingProfile : Profile
{
  public CatTagRequestToCatTagRequestMappingProfile()
  {
    CreateMap<CatRequest, CatTagRequest>()
      .ForMember(dest => dest.Cat, opt => opt.MapFrom(src => src))
      .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.tagRequests))
      ;
  }
}
