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
                .ForMember(dest => dest.Genre, opt => opt.Ignore());

            CreateMap<AuthorEntity, BookAuthorDTO>();
            CreateMap<GenreEntity, BookGenreDTO>();
            CreateMap<BookEntity, BookDTO>();

            CreateMap<AuthorCreateDTO, AuthorEntity>();

            CreateMap<BookEntity, AuthorBookDTO>();
            CreateMap<AuthorEntity, AuthorDTO>();
        }
    }
}
