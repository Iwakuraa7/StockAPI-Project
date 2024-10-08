using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Comment;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace api.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDBContext _context;

        public CommentRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            await _context.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id)
        {
            var commentToDelete = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            if(commentToDelete == null)
                return null;

            _context.Remove(commentToDelete);
            await _context.SaveChangesAsync();

            return commentToDelete;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.Include(a => a.AppUser).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetComments()
        {
            return await _context.Comments.Include(a => a.AppUser).ToListAsync();
        }

        public async Task<Comment?> UpdateAsync(int id, UpdateCommentDto commentDto)
        {
            var commentToUpdate = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);

            if(commentToUpdate == null)
                return null;

            commentToUpdate.Title = commentDto.Title;
            commentToUpdate.Content = commentDto.Content;

            await _context.SaveChangesAsync();
            return commentToUpdate;
        }
    }
}