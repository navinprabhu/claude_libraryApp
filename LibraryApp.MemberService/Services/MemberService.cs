using AutoMapper;
using LibraryApp.MemberService.Data.Repositories;
using LibraryApp.MemberService.Models.Entities;
using LibraryApp.MemberService.Models.Requests;
using LibraryApp.MemberService.Services.External;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.MemberService.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IBookServiceClient _bookServiceClient;
        private readonly IMapper _mapper;
        private readonly ILogger<MemberService> _logger;

        public MemberService(
            IMemberRepository memberRepository,
            IBookServiceClient bookServiceClient,
            IMapper mapper,
            ILogger<MemberService> logger)
        {
            _memberRepository = memberRepository;
            _bookServiceClient = bookServiceClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<MemberDto>>> GetAllMembersAsync()
        {
            try
            {
                var members = await _memberRepository.GetAllAsync();
                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(members);
                return ApiResponse<IEnumerable<MemberDto>>.SuccessResponse(memberDtos, "Members retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all members");
                return ApiResponse<IEnumerable<MemberDto>>.ErrorResponse("Failed to retrieve members", 500);
            }
        }

        public async Task<ApiResponse<MemberDto>> GetMemberByIdAsync(int id)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member not found", 404);
                }

                var memberDto = _mapper.Map<MemberDto>(member);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving member with ID: {MemberId}", id);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to retrieve member", 500);
            }
        }

        public async Task<ApiResponse<MemberDto>> GetMemberByEmailAsync(string email)
        {
            try
            {
                var member = await _memberRepository.GetByEmailAsync(email);
                if (member == null)
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member not found", 404);
                }

                var memberDto = _mapper.Map<MemberDto>(member);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving member with email: {Email}", email);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to retrieve member", 500);
            }
        }

        public async Task<ApiResponse<PagedResult<MemberDto>>> GetMembersPagedAsync(int page, int pageSize, string? searchTerm = null, MemberStatus? status = null)
        {
            try
            {
                var pagedMembers = await _memberRepository.GetPagedAsync(page, pageSize, searchTerm, status);
                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(pagedMembers.Items);
                
                var pagedResult = new PagedResult<MemberDto>
                {
                    Items = memberDtos.ToList(),
                    TotalCount = pagedMembers.TotalCount,
                    PageNumber = pagedMembers.PageNumber,
                    PageSize = pagedMembers.PageSize
                };

                return ApiResponse<PagedResult<MemberDto>>.SuccessResponse(pagedResult, "Members retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged members");
                return ApiResponse<PagedResult<MemberDto>>.ErrorResponse("Failed to retrieve members", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<MemberDto>>> SearchMembersAsync(string searchTerm)
        {
            try
            {
                var members = await _memberRepository.SearchMembersAsync(searchTerm);
                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(members);
                return ApiResponse<IEnumerable<MemberDto>>.SuccessResponse(memberDtos, "Members retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching members with term: {SearchTerm}", searchTerm);
                return ApiResponse<IEnumerable<MemberDto>>.ErrorResponse("Failed to search members", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<MemberDto>>> GetMembersByStatusAsync(MemberStatus status)
        {
            try
            {
                var members = await _memberRepository.GetMembersByStatusAsync(status);
                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(members);
                return ApiResponse<IEnumerable<MemberDto>>.SuccessResponse(memberDtos, "Members retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving members by status: {Status}", status);
                return ApiResponse<IEnumerable<MemberDto>>.ErrorResponse("Failed to retrieve members", 500);
            }
        }

        public async Task<ApiResponse<MemberDto>> CreateMemberAsync(CreateMemberRequest request, string createdBy)
        {
            try
            {
                if (await _memberRepository.EmailExistsAsync(request.Email))
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member with this email already exists", 400);
                }

                var member = _mapper.Map<Member>(request);
                member.CreatedBy = createdBy;
                member.CreatedAt = DateTime.UtcNow;
                member.IsActive = true;
                member.Status = MemberStatus.Active;
                member.JoinedDate = DateTime.UtcNow;

                var createdMember = await _memberRepository.CreateAsync(member);
                var memberDto = _mapper.Map<MemberDto>(createdMember);

                _logger.LogInformation("Member created successfully: {MemberId} by {CreatedBy}", createdMember.Id, createdBy);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating member: {Email}", request.Email);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to create member", 500);
            }
        }

        public async Task<ApiResponse<MemberDto>> UpdateMemberAsync(int id, UpdateMemberRequest request, string updatedBy)
        {
            try
            {
                var existingMember = await _memberRepository.GetByIdAsync(id);
                if (existingMember == null)
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member not found", 404);
                }

                if (!string.IsNullOrEmpty(request.Email) && 
                    request.Email != existingMember.Email && 
                    await _memberRepository.EmailExistsAsync(request.Email, id))
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member with this email already exists", 400);
                }

                _mapper.Map(request, existingMember);
                existingMember.UpdatedBy = updatedBy;
                existingMember.UpdatedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(existingMember);
                var memberDto = _mapper.Map<MemberDto>(existingMember);

                _logger.LogInformation("Member updated successfully: {MemberId} by {UpdatedBy}", id, updatedBy);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member: {MemberId}", id);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to update member", 500);
            }
        }

        public async Task<ApiResponse<MemberDto>> UpdateMemberStatusAsync(int id, MemberStatusUpdateRequest request, string updatedBy)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    return ApiResponse<MemberDto>.ErrorResponse("Member not found", 404);
                }

                member.Status = request.Status;
                member.UpdatedBy = updatedBy;
                member.UpdatedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(member);
                var memberDto = _mapper.Map<MemberDto>(member);

                _logger.LogInformation("Member status updated successfully: {MemberId} to {Status} by {UpdatedBy}", 
                    id, request.Status, updatedBy);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member status: {MemberId}", id);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to update member status", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeactivateMemberAsync(int id, string deactivatedBy)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Member not found", 404);
                }

                // Check if member has active borrowings
                var borrowedBooks = await GetMemberBorrowedBooksAsync(id);
                if (borrowedBooks.Success && borrowedBooks.Data != null && borrowedBooks.Data.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot deactivate member with active borrowings", 400);
                }

                member.IsActive = false;
                member.Status = MemberStatus.Inactive;
                member.UpdatedBy = deactivatedBy;
                member.UpdatedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(member);

                _logger.LogInformation("Member deactivated successfully: {MemberId} by {DeactivatedBy}", id, deactivatedBy);
                return ApiResponse<bool>.SuccessResponse(true, "Member deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating member: {MemberId}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to deactivate member", 500);
            }
        }

        public async Task<ApiResponse<bool>> CanMemberBorrowBooksAsync(int memberId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Member not found", 404);
                }

                if (member.Status != MemberStatus.Active)
                {
                    return ApiResponse<bool>.SuccessResponse(false, "Member is not active");
                }

                // Check current borrowing count
                var borrowedBooks = await GetMemberBorrowedBooksAsync(memberId);
                if (!borrowedBooks.Success)
                {
                    return ApiResponse<bool>.ErrorResponse("Failed to check member's current borrowings", 500);
                }

                var currentBorrowingCount = borrowedBooks.Data?.Count() ?? 0;
                var canBorrow = currentBorrowingCount < member.MaxBooksAllowed;

                var message = canBorrow 
                    ? $"Member can borrow more books ({currentBorrowingCount}/{member.MaxBooksAllowed})"
                    : $"Member has reached borrowing limit ({currentBorrowingCount}/{member.MaxBooksAllowed})";

                return ApiResponse<bool>.SuccessResponse(canBorrow, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if member can borrow books: {MemberId}", memberId);
                return ApiResponse<bool>.ErrorResponse("Failed to check borrowing eligibility", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowedBooksAsync(int memberId)
        {
            try
            {
                return await _bookServiceClient.GetMemberBorrowedBooksAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member borrowed books: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowed books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingHistoryAsync(int memberId)
        {
            try
            {
                return await _bookServiceClient.GetMemberBorrowingHistoryAsync(memberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member borrowing history: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowing history", 500);
            }
        }
    }
}