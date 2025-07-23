using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Repositories;
using LibraryApp.Shared.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.BookService.Data.Repositories
{
    public class BorrowingRepository : BaseRepository<BorrowingRecord>, IBorrowingRepository
    {
        private new readonly BookDbContext _context;

        public BorrowingRepository(BookDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BorrowingRecord>> GetByMemberIdAsync(int memberId)
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => br.MemberId == memberId)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetByBookIdAsync(int bookId)
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => br.BookId == bookId)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetActiveBorrowingsAsync()
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => !br.IsReturned)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetOverdueBorrowingsAsync()
        {
            var currentDate = DateTime.UtcNow;
            
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => !br.IsReturned && br.DueDate < currentDate)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetBorrowingHistoryAsync(int memberId)
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => br.MemberId == memberId)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }

        public async Task<BorrowingRecord?> GetActiveBorrowingAsync(int bookId, int memberId)
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => 
                    br.BookId == bookId && 
                    br.MemberId == memberId && 
                    !br.IsReturned);
        }

        public async Task<PagedResult<BorrowingRecord>> GetPagedAsync(int page, int pageSize, int? memberId = null, bool? isReturned = null)
        {
            var query = _context.BorrowingRecords.Include(br => br.Book).AsQueryable();

            if (memberId.HasValue)
            {
                query = query.Where(br => br.MemberId == memberId.Value);
            }

            if (isReturned.HasValue)
            {
                query = query.Where(br => br.IsReturned == isReturned.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(br => br.BorrowedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<BorrowingRecord>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> HasActiveBorrowingAsync(int bookId, int memberId)
        {
            return await _context.BorrowingRecords
                .AnyAsync(br => 
                    br.BookId == bookId && 
                    br.MemberId == memberId && 
                    !br.IsReturned);
        }

        public async Task<int> GetActiveBorrowingCountAsync(int memberId)
        {
            return await _context.BorrowingRecords
                .CountAsync(br => br.MemberId == memberId && !br.IsReturned);
        }

        public async Task<IEnumerable<BorrowingRecord>> GetDueSoonAsync(int daysAhead = 3)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);
            
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .Where(br => 
                    !br.IsReturned && 
                    br.DueDate <= targetDate && 
                    br.DueDate >= DateTime.UtcNow)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public override async Task<BorrowingRecord?> GetByIdAsync(int id)
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.Id == id);
        }

        public override async Task<IEnumerable<BorrowingRecord>> GetAllAsync()
        {
            return await _context.BorrowingRecords
                .Include(br => br.Book)
                .OrderByDescending(br => br.BorrowedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowingRecord>> GetRecentBorrowingsAsync(int limit = 10)
        {
            return await DbSet
                .OrderByDescending(b => b.BorrowedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<BorrowingRecord> CreateAsync(BorrowingRecord borrowingRecord)
        {
            return await AddAsync(borrowingRecord);
        }

        public new async Task<BorrowingRecord> UpdateAsync(BorrowingRecord entity)
        {
            await base.UpdateAsync(entity);
            return entity;
        }
    }
}