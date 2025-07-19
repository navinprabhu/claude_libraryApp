using LibraryApp.MemberService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Repositories;
using LibraryApp.Shared.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.MemberService.Data.Repositories
{
    public class MemberRepository : BaseRepository<Member>, IMemberRepository
    {
        private new readonly MemberDbContext _context;

        public MemberRepository(MemberDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Member?> GetByEmailAsync(string email)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Email == email && m.IsActive);
        }

        public async Task<IEnumerable<Member>> GetByNameAsync(string firstName, string lastName)
        {
            return await _context.Members
                .Where(m => m.FirstName.Contains(firstName) && 
                           m.LastName.Contains(lastName) && 
                           m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> SearchMembersAsync(string searchTerm)
        {
            return await _context.Members
                .Where(m => (m.FirstName.Contains(searchTerm) ||
                            m.LastName.Contains(searchTerm) ||
                            m.Email.Contains(searchTerm) ||
                            (m.PhoneNumber != null && m.PhoneNumber.Contains(searchTerm))) &&
                            m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<PagedResult<Member>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, MemberStatus? status = null)
        {
            var query = _context.Members.Where(m => m.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => 
                    m.FirstName.Contains(searchTerm) ||
                    m.LastName.Contains(searchTerm) ||
                    m.Email.Contains(searchTerm) ||
                    (m.PhoneNumber != null && m.PhoneNumber.Contains(searchTerm)));
            }

            if (status.HasValue)
            {
                query = query.Where(m => m.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Member>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Member>> GetMembersByStatusAsync(MemberStatus status)
        {
            return await _context.Members
                .Where(m => m.Status == status && m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetActiveMembersAsync()
        {
            return await _context.Members
                .Where(m => m.Status == MemberStatus.Active && m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetInactiveMembersAsync()
        {
            return await _context.Members
                .Where(m => m.Status == MemberStatus.Inactive || !m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Member>> GetMembersByMembershipTypeAsync(string membershipType)
        {
            return await _context.Members
                .Where(m => m.MembershipType == membershipType && m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Members
                .AnyAsync(m => m.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email, int excludeMemberId)
        {
            return await _context.Members
                .AnyAsync(m => m.Email == email && m.Id != excludeMemberId);
        }

        public async Task<int> GetTotalActiveMembersAsync()
        {
            return await _context.Members
                .CountAsync(m => m.Status == MemberStatus.Active && m.IsActive);
        }

        public async Task<Member> CreateAsync(Member member)
        {
            return await AddAsync(member);
        }

        public new async Task<Member> UpdateAsync(Member member)
        {
            await base.UpdateAsync(member);
            return member;
        }

        public override async Task<Member?> GetByIdAsync(int id)
        {
            return await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        public override async Task<IEnumerable<Member>> GetAllAsync()
        {
            return await _context.Members
                .Where(m => m.IsActive)
                .OrderBy(m => m.FirstName)
                .ThenBy(m => m.LastName)
                .ToListAsync();
        }
    }
}