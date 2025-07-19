using LibraryApp.BookService.Infrastructure.Authorization;
using LibraryApp.BookService.Models.Requests;
using LibraryApp.BookService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApp.BookService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeRoles(UserRoles.Admin, UserRoles.Member)]
    public class BorrowingsController : ControllerBase
    {
        private readonly IBorrowingService _borrowingService;
        private readonly ILogger<BorrowingsController> _logger;

        public BorrowingsController(IBorrowingService borrowingService, ILogger<BorrowingsController> logger)
        {
            _borrowingService = borrowingService;
            _logger = logger;
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole == UserRoles.Member && request.MemberId.ToString() != currentUserId)
            {
                return Forbid("Members can only borrow books for themselves");
            }

            var borrowedBy = GetCurrentUsername();
            var result = await _borrowingService.BorrowBookAsync(request, borrowedBy);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(nameof(GetBorrowingById), new { id = result.Data.Id }, result);
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var returnedBy = GetCurrentUsername();
            var result = await _borrowingService.ReturnBookAsync(request, returnedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("extend")]
        public async Task<IActionResult> ExtendBorrowing([FromBody] ExtendBorrowingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var extendedBy = GetCurrentUsername();
            var result = await _borrowingService.ExtendBorrowingAsync(request, extendedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBorrowingById(int id)
        {
            var result = await _borrowingService.GetBorrowingByIdAsync(id);
            
            if (result.Success && result.Data != null)
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                
                if (currentUserRole == UserRoles.Member && result.Data.MemberId.ToString() != currentUserId)
                {
                    return Forbid("Members can only view their own borrowing records");
                }
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("member/{memberId:int}")]
        public async Task<IActionResult> GetMemberBorrowings(int memberId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole == UserRoles.Member && memberId.ToString() != currentUserId)
            {
                return Forbid("Members can only view their own borrowing records");
            }

            var result = await _borrowingService.GetMemberBorrowingsAsync(memberId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("book/{bookId:int}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetBookBorrowings(int bookId)
        {
            var result = await _borrowingService.GetBookBorrowingsAsync(bookId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("active")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetActiveBorrowings()
        {
            var result = await _borrowingService.GetActiveBorrowingsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("overdue")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetOverdueBorrowings()
        {
            var result = await _borrowingService.GetOverdueBorrowingsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("due-soon")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetBorrowingsDueSoon([FromQuery] int daysAhead = 3)
        {
            if (daysAhead < 1 || daysAhead > 30)
            {
                return BadRequest(new { error = "Days ahead must be between 1 and 30" });
            }

            var result = await _borrowingService.GetDueSoonAsync(daysAhead);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("history/{memberId:int}")]
        public async Task<IActionResult> GetBorrowingHistory(int memberId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole == UserRoles.Member && memberId.ToString() != currentUserId)
            {
                return Forbid("Members can only view their own borrowing history");
            }

            var result = await _borrowingService.GetBorrowingHistoryAsync(memberId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("paged")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetBorrowingsPagedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? memberId = null,
            [FromQuery] bool? isReturned = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _borrowingService.GetBorrowingsPagedAsync(page, pageSize, memberId, isReturned);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("member/{memberId:int}/active-count")]
        public async Task<IActionResult> GetActiveBorrowingCount(int memberId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole == UserRoles.Member && memberId.ToString() != currentUserId)
            {
                return Forbid("Members can only view their own borrowing count");
            }

            var result = await _borrowingService.GetActiveBorrowingCountAsync(memberId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("member/{memberId:int}/can-borrow")]
        public async Task<IActionResult> CanMemberBorrow(int memberId, [FromQuery] int maxBooks = 5)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();
            
            if (currentUserRole == UserRoles.Member && memberId.ToString() != currentUserId)
            {
                return Forbid("Members can only check their own borrowing eligibility");
            }

            if (maxBooks < 1 || maxBooks > 20)
            {
                return BadRequest(new { error = "Max books must be between 1 and 20" });
            }

            var result = await _borrowingService.CanMemberBorrowAsync(memberId, maxBooks);
            return StatusCode(result.StatusCode, result);
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
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