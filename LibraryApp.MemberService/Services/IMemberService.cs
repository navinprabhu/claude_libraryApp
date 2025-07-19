using LibraryApp.MemberService.Models.Entities;
using LibraryApp.MemberService.Models.Requests;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.MemberService.Services
{
    public interface IMemberService
    {
        Task<ApiResponse<IEnumerable<MemberDto>>> GetAllMembersAsync();
        Task<ApiResponse<MemberDto>> GetMemberByIdAsync(int id);
        Task<ApiResponse<MemberDto>> GetMemberByEmailAsync(string email);
        Task<ApiResponse<PagedResult<MemberDto>>> GetMembersPagedAsync(int page, int pageSize, string? searchTerm = null, MemberStatus? status = null);
        Task<ApiResponse<IEnumerable<MemberDto>>> SearchMembersAsync(string searchTerm);
        Task<ApiResponse<IEnumerable<MemberDto>>> GetMembersByStatusAsync(MemberStatus status);
        Task<ApiResponse<MemberDto>> CreateMemberAsync(CreateMemberRequest request, string createdBy);
        Task<ApiResponse<MemberDto>> UpdateMemberAsync(int id, UpdateMemberRequest request, string updatedBy);
        Task<ApiResponse<MemberDto>> UpdateMemberStatusAsync(int id, MemberStatusUpdateRequest request, string updatedBy);
        Task<ApiResponse<bool>> DeactivateMemberAsync(int id, string deactivatedBy);
        Task<ApiResponse<bool>> CanMemberBorrowBooksAsync(int memberId);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowedBooksAsync(int memberId);
        Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingHistoryAsync(int memberId);
    }
}