using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Models.Common;

namespace LibraryApp.BookService.Data.Repositories
{
    public interface IBorrowingRepository : IRepository<BorrowingRecord>
    {
        Task<IEnumerable<BorrowingRecord>> GetByMemberIdAsync(int memberId);
        Task<IEnumerable<BorrowingRecord>> GetByBookIdAsync(int bookId);
        Task<IEnumerable<BorrowingRecord>> GetActiveBorrowingsAsync();
        Task<IEnumerable<BorrowingRecord>> GetOverdueBorrowingsAsync();
        Task<IEnumerable<BorrowingRecord>> GetBorrowingHistoryAsync(int memberId);
        Task<BorrowingRecord?> GetActiveBorrowingAsync(int bookId, int memberId);
        Task<PagedResult<BorrowingRecord>> GetPagedAsync(int page, int pageSize, int? memberId = null, bool? isReturned = null);
        Task<bool> HasActiveBorrowingAsync(int bookId, int memberId);
        Task<int> GetActiveBorrowingCountAsync(int memberId);
        Task<IEnumerable<BorrowingRecord>> GetDueSoonAsync(int daysAhead = 3);
        Task<IEnumerable<BorrowingRecord>> GetRecentBorrowingsAsync(int limit = 10);
        Task<BorrowingRecord> CreateAsync(BorrowingRecord borrowingRecord);
        new Task<BorrowingRecord> UpdateAsync(BorrowingRecord borrowingRecord);
    }
}