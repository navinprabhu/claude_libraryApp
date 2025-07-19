using LibraryApp.MemberService.Infrastructure.Authorization;
using LibraryApp.MemberService.Models.Entities;
using LibraryApp.MemberService.Models.Requests;
using LibraryApp.MemberService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApp.MemberService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly ILogger<MembersController> _logger;

        public MembersController(IMemberService memberService, ILogger<MembersController> logger)
        {
            _memberService = memberService;
            _logger = logger;
        }

        [HttpGet]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetAllMembers()
        {
            var result = await _memberService.GetAllMembersAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("paged")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetMembersPagedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] MemberStatus? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _memberService.GetMembersPagedAsync(page, pageSize, searchTerm, status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
        public async Task<IActionResult> GetMemberById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            // Members can only access their own data
            if (currentUserRole == UserRoles.Member && id.ToString() != currentUserId)
            {
                return Forbid("Members can only access their own information");
            }

            var result = await _memberService.GetMemberByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("email/{email}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetMemberByEmail(string email)
        {
            var result = await _memberService.GetMemberByEmailAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("search")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> SearchMembers([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            var result = await _memberService.SearchMembersAsync(searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("status/{status}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetMembersByStatus(MemberStatus status)
        {
            var result = await _memberService.GetMembersByStatusAsync(status);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBy = GetCurrentUsername() ?? "Anonymous";
            var result = await _memberService.CreateMemberAsync(request, createdBy);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(nameof(GetMemberById), new { id = result.Data.Id }, result);
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
        public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            // Members can only update their own data and limited fields
            if (currentUserRole == UserRoles.Member)
            {
                if (id.ToString() != currentUserId)
                {
                    return Forbid("Members can only update their own information");
                }

                // Members cannot change status, membership type, or max books allowed
                if (request.Status.HasValue || 
                    !string.IsNullOrEmpty(request.MembershipType) || 
                    request.MaxBooksAllowed.HasValue)
                {
                    return Forbid("Members cannot modify these fields");
                }
            }

            var updatedBy = GetCurrentUsername() ?? "Unknown";
            var result = await _memberService.UpdateMemberAsync(id, request, updatedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}/status")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> UpdateMemberStatus(int id, [FromBody] MemberStatusUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = GetCurrentUsername() ?? "Unknown";
            var result = await _memberService.UpdateMemberStatusAsync(id, request, updatedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> DeactivateMember(int id)
        {
            var deactivatedBy = GetCurrentUsername() ?? "Unknown";
            var result = await _memberService.DeactivateMemberAsync(id, deactivatedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/borrowed-books")]
        [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
        public async Task<IActionResult> GetMemberBorrowedBooks(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            // Members can only access their own borrowing data
            if (currentUserRole == UserRoles.Member && id.ToString() != currentUserId)
            {
                return Forbid("Members can only access their own borrowing information");
            }

            var result = await _memberService.GetMemberBorrowedBooksAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/borrowing-history")]
        [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
        public async Task<IActionResult> GetMemberBorrowingHistory(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            // Members can only access their own borrowing history
            if (currentUserRole == UserRoles.Member && id.ToString() != currentUserId)
            {
                return Forbid("Members can only access their own borrowing history");
            }

            var result = await _memberService.GetMemberBorrowingHistoryAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/can-borrow")]
        [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
        public async Task<IActionResult> CanMemberBorrowBooks(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            // Members can only check their own borrowing eligibility
            if (currentUserRole == UserRoles.Member && id.ToString() != currentUserId)
            {
                return Forbid("Members can only check their own borrowing eligibility");
            }

            var result = await _memberService.CanMemberBorrowBooksAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        private string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}