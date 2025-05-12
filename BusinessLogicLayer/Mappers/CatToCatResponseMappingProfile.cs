using AutoMapper;
using DataAccessLayer.Entities;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class CatToCatResponseMappingProfile : Profile
{
  public CatToCatResponseMappingProfile()
  {
        CreateMap<Cat, CatResponse>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.CatId))
        .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
        .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
        .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
        .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
        .ForMember(dest => dest.TagResponses, opt => opt.MapFrom(src =>
            src.CatTags.Select(ct => new TagResponse
            {
                Name = ct.Tag.Name
            }).ToList()
    
        ));

    }
}
