using backend.Model.dto.Comment;
using Backend.Model.Entity;
using Backend.Repository.CommentRepository;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Service.CommentService
{
    public interface ICommentService
    {
        Task CreateAsync(Guid customerId, CreateCommentDto dto);
        Task UpdateAsync(int id, Guid customerId, UpdateCommentDto dto);
        Task DeleteAsync(int id, Guid customerId); // ĐÃ SỬA: bỏ UpdateCommentDto
        Task<List<CommentDto>> GetByProductIdAsync(long productId);
        Task<List<CommentDto>> GetNextCommentsAsync(long productId, int? lastCommentId, int pageSize);
        Task<List<CommentDto>> GetMyCommentsForProductAsync(Guid customerId, long productId);
    }

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly SQLServerDbContext _context;

        public CommentService(ICommentRepository repo, SQLServerDbContext context)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // === TẠO COMMENT ===
        public async Task CreateAsync(Guid customerId, CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new ArgumentException("Nội dung không được để trống.");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new ArgumentException("Đánh giá từ 1-5 sao.");

            var comment = new Comment
            {
                Content = dto.Content,
                Rating = dto.Rating,
                CreatedAt = DateTime.UtcNow,
                CustomerId = customerId,
                ProductId = dto.ProductId
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repo.CreateAsync(comment);
                await RecalculateProductRatingAsync(dto.ProductId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // === SỬA COMMENT ===
        public async Task UpdateAsync(int id, Guid customerId, UpdateCommentDto dto)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) throw new KeyNotFoundException("Không tìm thấy bình luận.");
            if (comment.CustomerId != customerId) throw new UnauthorizedAccessException("Không phải bình luận của bạn.");

            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new ArgumentException("Nội dung không được để trống.");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new ArgumentException("Đánh giá từ 1-5 sao.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                comment.Content = dto.Content;
                comment.Rating = dto.Rating;
                await _repo.UpdateAsync(comment);
                await RecalculateProductRatingAsync(comment.ProductId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // === XÓA COMMENT ===
        public async Task DeleteAsync(int id, Guid customerId)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) throw new KeyNotFoundException("Không tìm thấy bình luận.");
            if (comment.CustomerId != customerId) throw new UnauthorizedAccessException("Không phải bình luận của bạn.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _repo.DeleteAsync(id);
                await RecalculateProductRatingAsync(comment.ProductId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // === LẤY TẤT CẢ COMMENT CỦA SẢN PHẨM ===
        public async Task<List<CommentDto>> GetByProductIdAsync(long productId)
        {
            var comments = await _repo.GetByProductIdAsync(productId);
            return comments.Select(MapToDto).ToList();
        }

        // === LẤY 5 COMMENT TIẾP THEO ===
        public async Task<List<CommentDto>> GetNextCommentsAsync(long productId, int? lastCommentId, int pageSize)
        {
            var comments = await _repo.GetNextCommentsAsync(productId, lastCommentId, pageSize);
            return comments.Select(MapToDto).ToList();
        }

        // === LẤY COMMENT CỦA CHÍNH MÌNH ===
        public async Task<List<CommentDto>> GetMyCommentsForProductAsync(Guid customerId, long productId)
        {
            var comments = await _repo.GetCustomerCommentsForProductAsync(customerId, productId);
            return comments.Select(MapToDto).ToList();
        }

        // === TÍNH LẠI RATING SẢN PHẨM ===
        private async Task RecalculateProductRatingAsync(long productId)
        {
            var comments = await _context.Comments
                .Where(c => c.ProductId == productId)
                .Select(c => c.Rating)
                .ToListAsync();

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            if (comments.Any())
            {
                product.TotalRatings = comments.Count;
                product.Rating = (float)Math.Round(comments.Average(), 2);
            }
            else
            {
                product.TotalRatings = 0;
                product.Rating = 0.0f;
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // === CHUYỂN ENTITY → DTO ===
        private CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Rating = comment.Rating,
                CreatedAt = comment.CreatedAt,
                CustomerName = comment.Customer?.CustomerName ?? "Ẩn danh"
            };
        }
    }
}