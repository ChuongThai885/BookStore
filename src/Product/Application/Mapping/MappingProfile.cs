using AutoMapper;
using Product.Application.DTOs;
using Product.Domain.Entity;

namespace Product.Application.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<BookCreateDTO, BookEntity>()
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Genres, opt => opt.Ignore());

            CreateMap<AuthorEntity, BookAuthorDTO>();
            CreateMap<GenreEntity, BookGenreDTO>();
            CreateMap<BookEntity, BookDTO>()
                .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => src.Genres ));

            CreateMap<AuthorCreateDTO, AuthorEntity>();

            CreateMap<BookEntity, AuthorBookDTO>()
                .ForMember(dest => dest.Genres,
                 opt => opt.MapFrom(src => src.Genres));
            CreateMap<AuthorEntity, AuthorDTO>();

            CreateMap<GenreEntity, GenreDTO>()
                .ForMember(dest => dest.Books, opt => opt.MapFrom(src => src.Books));
            CreateMap<BookEntity, GenreBookDTO>();
        }
    }
}
