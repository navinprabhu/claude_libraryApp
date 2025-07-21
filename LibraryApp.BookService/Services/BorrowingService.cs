using AutoMapper;
using LibraryApp.BookService.Data.Repositories;
using LibraryApp.BookService.Models.Entities;
using LibraryApp.BookService.Models.Requests;
using LibraryApp.Shared.Events;
using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Infrastructure.Middleware;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.BookService.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BorrowingService> _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICorrelationIdService _correlationIdService;
        private const int DefaultBorrowingPeriodDays = 14;
        private const int MaxBooksPerMember = 5;

        public BorrowingService(
            IBorrowingRepository borrowingRepository,
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<BorrowingService> logger,
            IEventPublisher eventPublisher,
            ICorrelationIdService correlationIdService)
        {
            _borrowingRepository = borrowingRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _correlationIdService = correlationIdService;
        }

        public async Task<ApiResponse<BorrowingRecordDto>> BorrowBookAsync(BorrowBookRequest request, string borrowedBy)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(request.BookId);
                if (book == null)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Book not found", 404);
                }

                if (!await _bookRepository.HasAvailableCopiesAsync(request.BookId))
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Book is not available for borrowing", 400);
                }

                if (await _borrowingRepository.HasActiveBorrowingAsync(request.BookId, request.MemberId))
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Member already has this book borrowed", 400);
                }

                var activeBorrowingCount = await _borrowingRepository.GetActiveBorrowingCountAsync(request.MemberId);
                if (activeBorrowingCount >= MaxBooksPerMember)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse($"Member has reached maximum borrowing limit of {MaxBooksPerMember} books", 400);
                }

                var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(DefaultBorrowingPeriodDays);
                
                var borrowingRecord = new BorrowingRecord
                {
                    BookId = request.BookId,
                    MemberId = request.MemberId,
                    MemberName = request.MemberName,
                    MemberEmail = request.MemberEmail,
                    BorrowedAt = DateTime.UtcNow,
                    DueDate = dueDate,
                    Notes = request.Notes,
                    CreatedBy = borrowedBy,
                    CreatedAt = DateTime.UtcNow
                };

                var createdRecord = await _borrowingRepository.CreateAsync(borrowingRecord);
                await _bookRepository.UpdateAvailableCopiesAsync(request.BookId, -1);

                var recordDto = _mapper.Map<BorrowingRecordDto>(createdRecord);

                // Publish BookBorrowedEvent
                var bookBorrowedEvent = new BookBorrowedEvent
                {
                    BorrowingRecordId = createdRecord.Id,
                    BookId = createdRecord.BookId,
                    MemberId = createdRecord.MemberId,
                    BorrowDate = createdRecord.BorrowedAt,
                    DueDate = createdRecord.DueDate,
                    BookTitle = book.Title,
                    MemberEmail = createdRecord.MemberEmail,
                    CorrelationId = _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString()
                };

                try
                {
                    await _eventPublisher.PublishAsync(bookBorrowedEvent);
                    _logger.LogInformation("BookBorrowedEvent published for BorrowingRecord {BorrowingRecordId}", 
                        createdRecord.Id);
                }
                catch (Exception eventEx)
                {
                    _logger.LogError(eventEx, "Failed to publish BookBorrowedEvent for BorrowingRecord {BorrowingRecordId}", 
                        createdRecord.Id);
                    // Don't fail the operation if event publishing fails
                }

                _logger.LogInformation("Book borrowed successfully: BookId {BookId} by Member {MemberId}", 
                    request.BookId, request.MemberId);

                return ApiResponse<BorrowingRecordDto>.SuccessResponse(recordDto, "Book borrowed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error borrowing book: BookId {BookId}, MemberId {MemberId}", 
                    request.BookId, request.MemberId);
                return ApiResponse<BorrowingRecordDto>.ErrorResponse("Failed to borrow book", 500);
            }
        }

        public async Task<ApiResponse<BorrowingRecordDto>> ReturnBookAsync(ReturnBookRequest request, string returnedBy)
        {
            try
            {
                var borrowingRecord = await _borrowingRepository.GetByIdAsync(request.BorrowingRecordId);
                if (borrowingRecord == null)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Borrowing record not found", 404);
                }

                if (borrowingRecord.IsReturned)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Book has already been returned", 400);
                }

                borrowingRecord.IsReturned = true;
                borrowingRecord.ReturnedAt = DateTime.UtcNow;
                borrowingRecord.Notes = !string.IsNullOrEmpty(request.Notes) 
                    ? $"{borrowingRecord.Notes}\n{request.Notes}" 
                    : borrowingRecord.Notes;
                borrowingRecord.LateFee = request.LateFee;
                borrowingRecord.UpdatedBy = returnedBy;
                borrowingRecord.UpdatedAt = DateTime.UtcNow;

                await _borrowingRepository.UpdateAsync(borrowingRecord);
                await _bookRepository.UpdateAvailableCopiesAsync(borrowingRecord.BookId, 1);

                var recordDto = _mapper.Map<BorrowingRecordDto>(borrowingRecord);

                // Get book details for the event
                var book = await _bookRepository.GetByIdAsync(borrowingRecord.BookId);

                // Publish BookReturnedEvent
                var bookReturnedEvent = new BookReturnedEvent
                {
                    BorrowingRecordId = borrowingRecord.Id,
                    BookId = borrowingRecord.BookId,
                    MemberId = borrowingRecord.MemberId,
                    ReturnDate = borrowingRecord.ReturnedAt ?? DateTime.UtcNow,
                    DueDate = borrowingRecord.DueDate,
                    IsLate = borrowingRecord.ReturnedAt > borrowingRecord.DueDate,
                    LateFee = borrowingRecord.LateFee,
                    BookTitle = book?.Title ?? "Unknown",
                    MemberEmail = borrowingRecord.MemberEmail,
                    CorrelationId = _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString()
                };

                try
                {
                    await _eventPublisher.PublishAsync(bookReturnedEvent);
                    _logger.LogInformation("BookReturnedEvent published for BorrowingRecord {BorrowingRecordId}", 
                        borrowingRecord.Id);
                }
                catch (Exception eventEx)
                {
                    _logger.LogError(eventEx, "Failed to publish BookReturnedEvent for BorrowingRecord {BorrowingRecordId}", 
                        borrowingRecord.Id);
                    // Don't fail the operation if event publishing fails
                }

                _logger.LogInformation("Book returned successfully: BorrowingRecordId {BorrowingRecordId}", 
                    request.BorrowingRecordId);

                return ApiResponse<BorrowingRecordDto>.SuccessResponse(recordDto, "Book returned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book: BorrowingRecordId {BorrowingRecordId}", 
                    request.BorrowingRecordId);
                return ApiResponse<BorrowingRecordDto>.ErrorResponse("Failed to return book", 500);
            }
        }

        public async Task<ApiResponse<BorrowingRecordDto>> ExtendBorrowingAsync(ExtendBorrowingRequest request, string extendedBy)
        {
            try
            {
                var borrowingRecord = await _borrowingRepository.GetByIdAsync(request.BorrowingRecordId);
                if (borrowingRecord == null)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Borrowing record not found", 404);
                }

                if (borrowingRecord.IsReturned)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Cannot extend returned book", 400);
                }

                if (request.NewDueDate <= DateTime.UtcNow)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("New due date must be in the future", 400);
                }

                borrowingRecord.DueDate = request.NewDueDate;
                borrowingRecord.Notes = !string.IsNullOrEmpty(request.Notes) 
                    ? $"{borrowingRecord.Notes}\nExtended: {request.Notes}" 
                    : $"{borrowingRecord.Notes}\nExtended on {DateTime.UtcNow:yyyy-MM-dd}";
                borrowingRecord.UpdatedBy = extendedBy;
                borrowingRecord.UpdatedAt = DateTime.UtcNow;

                await _borrowingRepository.UpdateAsync(borrowingRecord);
                var recordDto = _mapper.Map<BorrowingRecordDto>(borrowingRecord);

                _logger.LogInformation("Borrowing extended successfully: BorrowingRecordId {BorrowingRecordId}", 
                    request.BorrowingRecordId);

                return ApiResponse<BorrowingRecordDto>.SuccessResponse(recordDto, "Borrowing period extended successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending borrowing: BorrowingRecordId {BorrowingRecordId}", 
                    request.BorrowingRecordId);
                return ApiResponse<BorrowingRecordDto>.ErrorResponse("Failed to extend borrowing", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingsAsync(int memberId)
        {
            try
            {
                var borrowings = await _borrowingRepository.GetByMemberIdAsync(memberId);
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, "Member borrowings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving member borrowings: MemberId {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve member borrowings", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetBookBorrowingsAsync(int bookId)
        {
            try
            {
                var borrowings = await _borrowingRepository.GetByBookIdAsync(bookId);
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, "Book borrowings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book borrowings: BookId {BookId}", bookId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve book borrowings", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetActiveBorrowingsAsync()
        {
            try
            {
                var borrowings = await _borrowingRepository.GetActiveBorrowingsAsync();
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, "Active borrowings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active borrowings");
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve active borrowings", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetOverdueBorrowingsAsync()
        {
            try
            {
                var borrowings = await _borrowingRepository.GetOverdueBorrowingsAsync();
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, "Overdue borrowings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue borrowings");
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve overdue borrowings", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetBorrowingHistoryAsync(int memberId)
        {
            try
            {
                var borrowings = await _borrowingRepository.GetBorrowingHistoryAsync(memberId);
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, "Borrowing history retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowing history: MemberId {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowing history", 500);
            }
        }

        public async Task<ApiResponse<PagedResult<BorrowingRecordDto>>> GetBorrowingsPagedAsync(int page, int pageSize, int? memberId = null, bool? isReturned = null)
        {
            try
            {
                var pagedBorrowings = await _borrowingRepository.GetPagedAsync(page, pageSize, memberId, isReturned);
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(pagedBorrowings.Items);
                
                var pagedResult = new PagedResult<BorrowingRecordDto>
                {
                    Items = borrowingDtos.ToList(),
                    TotalCount = pagedBorrowings.TotalCount,
                    PageNumber = pagedBorrowings.PageNumber,
                    PageSize = pagedBorrowings.PageSize
                };

                return ApiResponse<PagedResult<BorrowingRecordDto>>.SuccessResponse(pagedResult, "Borrowings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged borrowings");
                return ApiResponse<PagedResult<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowings", 500);
            }
        }

        public async Task<ApiResponse<BorrowingRecordDto>> GetBorrowingByIdAsync(int id)
        {
            try
            {
                var borrowing = await _borrowingRepository.GetByIdAsync(id);
                if (borrowing == null)
                {
                    return ApiResponse<BorrowingRecordDto>.ErrorResponse("Borrowing record not found", 404);
                }

                var borrowingDto = _mapper.Map<BorrowingRecordDto>(borrowing);
                return ApiResponse<BorrowingRecordDto>.SuccessResponse(borrowingDto, "Borrowing record retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowing record: Id {Id}", id);
                return ApiResponse<BorrowingRecordDto>.ErrorResponse("Failed to retrieve borrowing record", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetDueSoonAsync(int daysAhead = 3)
        {
            try
            {
                var borrowings = await _borrowingRepository.GetDueSoonAsync(daysAhead);
                var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos, $"Borrowings due within {daysAhead} days retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrowings due soon");
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowings due soon", 500);
            }
        }

        public async Task<ApiResponse<int>> GetActiveBorrowingCountAsync(int memberId)
        {
            try
            {
                var count = await _borrowingRepository.GetActiveBorrowingCountAsync(memberId);
                return ApiResponse<int>.SuccessResponse(count, "Active borrowing count retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active borrowing count: MemberId {MemberId}", memberId);
                return ApiResponse<int>.ErrorResponse("Failed to retrieve active borrowing count", 500);
            }
        }

        public async Task<ApiResponse<bool>> CanMemberBorrowAsync(int memberId, int maxBooksPerMember = MaxBooksPerMember)
        {
            try
            {
                var activeBorrowingCount = await _borrowingRepository.GetActiveBorrowingCountAsync(memberId);
                var canBorrow = activeBorrowingCount < maxBooksPerMember;
                
                var message = canBorrow 
                    ? $"Member can borrow more books ({activeBorrowingCount}/{maxBooksPerMember})"
                    : $"Member has reached borrowing limit ({activeBorrowingCount}/{maxBooksPerMember})";

                return ApiResponse<bool>.SuccessResponse(canBorrow, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if member can borrow: MemberId {MemberId}", memberId);
                return ApiResponse<bool>.ErrorResponse("Failed to check borrowing eligibility", 500);
            }
        }

        //public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetActiveBorrowingsForBookAsync(int bookId)
        //{
        //    try
        //    {
        //        var borrowings = await _borrowingRepository.GetActiveBorrowingsForBookAsync(bookId);
        //        var borrowingDtos = _mapper.Map<IEnumerable<BorrowingRecordDto>>(borrowings);

        //        _logger.LogDebug("Retrieved {Count} active borrowings for book {BookId}", 
        //            borrowingDtos.Count(), bookId);

        //        return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingDtos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting active borrowings for book: BookId {BookId}", bookId);
        //        return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to get active borrowings", 500);
        //    }
        //}
    }
}