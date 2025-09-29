using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Auction>().ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<Auction, CreateAuctionDto>();
        CreateMap<UpdateAuctionDto, Auction>().ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
        CreateMap<UpdateAuctionDto, Item>()
        .ForMember(d => d.Make, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Make)))
        .ForMember(d => d.Model, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Model)))
        .ForMember(d => d.Color, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Color)))
        .ForMember(d => d.Year, opt => opt.Condition(s => s.Year.HasValue))
        .ForMember(d => d.Mileage, opt => opt.Condition(s => s.Mileage.HasValue));
    }
}
