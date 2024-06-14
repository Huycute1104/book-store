using AutoMapper;
using BookStore.Model;
using Repository.Models;
using System.Diagnostics.Metrics;

namespace BookStore.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryMapper>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName))
                .ReverseMap();
            CreateMap<Category, CreateCategoryMapper>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName))
                .ReverseMap();
        }

    }
}
