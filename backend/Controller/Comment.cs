using backend.Model.dto.Comment;
using Backend.Service.CommentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize(Roles = "Customer")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
        }

        private Guid GetCustomerId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));
        }

        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var customerId = GetCustomerId();
            await _commentService.CreateAsync(customerId, dto);
            return Ok(new { Message = "Bình luận đã được gửi thành công" });
        }

        // GET: api/comments/product/5?lastCommentId=10&pageSize=5
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(
            long productId,
            [FromQuery] int? lastCommentId,
            [FromQuery] int pageSize = 5)
        {
            var comments = await _commentService.GetNextCommentsAsync(productId, lastCommentId, pageSize);
            var hasMore = comments.Count == pageSize;
            return Ok(new
            {
                Message = "Lấy danh sách bình luận thành công",
                Data = new
                {
                    Comments = comments,
                    HasMore = hasMore,
                    NextLastCommentId = comments.Any() ? comments.Min(c => c.Id) : (int?)null
                }
            });
        }

        // GET: api/comments/my/product/5
        [HttpGet("my/product/{productId}")]
        public async Task<IActionResult> GetMyCommentsForProduct(long productId)
        {
            var customerId = GetCustomerId();
            var comments = await _commentService.GetMyCommentsForProductAsync(customerId, productId);
            return Ok(new
            {
                Message = "Lấy bình luận của bạn thành công",
                Data = comments
            });
        }

        // PUT: api/comments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            var customerId = GetCustomerId();
            await _commentService.UpdateAsync(id, customerId, dto);
            return Ok(new { Message = "Cập nhật bình luận thành công" });
        }

        // DELETE: api/comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var customerId = GetCustomerId();
            await _commentService.DeleteAsync(id, customerId);
            return Ok(new { Message = "Xóa bình luận thành công" });
        }
    }
}