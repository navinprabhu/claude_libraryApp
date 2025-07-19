using LibraryApp.MemberService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Models.Common;

namespace LibraryApp.MemberService.Data.Repositories
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<Member?> GetByEmailAsync(string email);
        Task<IEnumerable<Member>> GetByNameAsync(string firstName, string lastName);
        Task<IEnumerable<Member>> SearchMembersAsync(string searchTerm);
        Task<PagedResult<Member>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, MemberStatus? status = null);
        Task<IEnumerable<Member>> GetMembersByStatusAsync(MemberStatus status);
        Task<IEnumerable<Member>> GetActiveMembersAsync();
        Task<IEnumerable<Member>> GetInactiveMembersAsync();
        Task<IEnumerable<Member>> GetMembersByMembershipTypeAsync(string membershipType);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsAsync(string email, int excludeMemberId);
        Task<int> GetTotalActiveMembersAsync();
        Task<Member> CreateAsync(Member member);
        new Task<Member> UpdateAsync(Member member);
    }
}