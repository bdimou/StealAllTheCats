using AutoMapper;
using DataAccessLayer.Entities;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public class CatRequestToCatMappingProfile : Profile
{
  public CatRequestToCatMappingProfile()
  {
        CreateMap<CatRequest, Cat>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.CatId))
        .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
        .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
        .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
        .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
        .ForMember(dest => dest.CatTags, opt => opt.MapFrom(src =>
            src.tagRequests.Select(tagRequest => new CatTag
            {
                Tag = new Tag
                {
                    Name = tagRequest.Name,
                    Created = tagRequest.Created
                }
            }).ToList()
        ));

    }
}
