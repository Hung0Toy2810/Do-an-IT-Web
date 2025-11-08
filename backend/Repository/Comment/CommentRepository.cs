using Backend.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repository.CommentRepository
{
    public interface ICommentRepository
    {
        Task CreateAsync(Comment comment);
        Task<Comment?> GetByIdAsync(int id);
        Task<List<Comment>> GetByProductIdAsync(long productId);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
        Task<List<Comment>> GetNextCommentsAsync(long productId, int? lastCommentId, int pageSize);
        Task<List<Comment>> GetCustomerCommentsForProductAsync(Guid customerId, long productId);
    }

    public class CommentRepository : ICommentRepository
    {
        private readonly SQLServerDbContext _context;

        public CommentRepository(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync(); // ĐÚNG
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetByProductIdAsync(long productId)
        {
            return await _context.Comments
                .Include(c => c.Customer)
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync(); // ĐÃ SỬA: SaveChangesAsync
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync(); // ĐÚNG
            }
        }

        public async Task<List<Comment>> GetNextCommentsAsync(long productId, int? lastCommentId, int pageSize)
        {
            var query = _context.Comments
                .Include(c => c.Customer)
                .Where(c => c.ProductId == productId);

            if (lastCommentId.HasValue)
            {
                query = query.Where(c => c.Id < lastCommentId.Value);
            }

            return await query
                .OrderByDescending(c => c.Id)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetCustomerCommentsForProductAsync(Guid customerId, long productId)
        {
            return await _context.Comments
                .Include(c => c.Customer)
                .Where(c => c.CustomerId == customerId && c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}