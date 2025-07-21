using LibraryApp.BookService.Models.Requests;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.BookService.Services
{
    public interface IBorrowingService
    {
        Task<ApiResponse<BorrowingRecordDto>> BorrowBookAsync(BorrowBookRequest request, string borrowedBy);
        Task<ApiResponse<BorrowingRecordDto>> ReturnBookAsync(ReturnBookRequest request, string returnedBy);
        Task<ApiResponse<BorrowingRecordDto>> ExtendBorrowingAsync(ExtendBorrowingRequest request, string extendedBy);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingsAsync(int memberId);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetBookBorrowingsAsync(int bookId);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetActiveBorrowingsAsync();
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetOverdueBorrowingsAsync();
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetBorrowingHistoryAsync(int memberId);
        Task<ApiResponse<PagedResult<BorrowingRecordDto>>> GetBorrowingsPagedAsync(int page, int pageSize, int? memberId = null, bool? isReturned = null);
        Task<ApiResponse<BorrowingRecordDto>> GetBorrowingByIdAsync(int id);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetDueSoonAsync(int daysAhead = 3);
        Task<ApiResponse<int>> GetActiveBorrowingCountAsync(int memberId);
        Task<ApiResponse<bool>> CanMemberBorrowAsync(int memberId, int maxBooksPerMember = 5);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetActiveBorrowingsForBookAsync(int bookId);
    }
}