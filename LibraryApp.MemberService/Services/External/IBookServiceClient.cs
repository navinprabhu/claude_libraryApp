using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.MemberService.Services.External
{
    public interface IBookServiceClient
    {
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowedBooksAsync(int memberId);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingHistoryAsync(int memberId);
        Task<ApiResponse<int>> GetMemberActiveBorrowingCountAsync(int memberId);
        Task<ApiResponse<bool>> CanMemberBorrowAsync(int memberId, int maxBooks);
    }
}