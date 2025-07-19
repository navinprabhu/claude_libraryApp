using AutoMapper;
using LibraryApp.BookService.Models.Entities;
using LibraryApp.BookService.Models.Requests;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.BookService.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.PublishedYear.HasValue ? new DateTime(src.PublishedYear.Value, 1, 1) : DateTime.MinValue));

            CreateMap<CreateBookDto, Book>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.PublishedYear, opt => opt.MapFrom(src => src.PublishedDate.Year))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.AvailableCopies, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowingRecords, opt => opt.Ignore());

            CreateMap<UpdateBookDto, Book>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.PublishedYear, opt => opt.MapFrom(src => src.PublishedDate.HasValue ? src.PublishedDate.Value.Year : (int?)null))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.AvailableCopies, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowingRecords, opt => opt.Ignore());

            CreateMap<BorrowingRecord, BorrowingRecordDto>();

            CreateMap<BorrowBookRequest, BorrowingRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DueDate, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsReturned, opt => opt.Ignore())
                .ForMember(dest => dest.LateFee, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore());
        }
    }
}