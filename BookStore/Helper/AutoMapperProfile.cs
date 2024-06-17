using AutoMapper;
using BookStore.Model;
using Repository.Models;

namespace BookStore.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName));

            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BookName))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

            CreateMap<Image, ImageDto>();

            CreateMap<BookMapper, Book>()
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.BookName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.UnitsInStock, opt => opt.MapFrom(src => src.UnitsInStock))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .AfterMap((src, dest) =>
                {
                    dest.Images = src.Images.Select(file => new Image { Url = file.FileName }).ToList();
                });

            CreateMap<Category, CategoryMapper>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName))
                .ReverseMap();

            CreateMap<Category, CreateCategoryMapper>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName))
                .ReverseMap();

   
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.UserStatus, opt => opt.MapFrom(src => src.UserStatus))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

            CreateMap<User, UserMapper>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.UserStatus, opt => opt.MapFrom(src => src.UserStatus))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ReverseMap();

            CreateMap<Role, RoleDto>();



        }
    }
}
