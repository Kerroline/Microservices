using Microsoft.EntityFrameworkCore;
using MS_Posts.DAL;
using MS_Posts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MS_Posts.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostModel>> GetAllAsync();
        Task<IEnumerable<PostModel>> GetAllByUser(string userId);
        Task<PostModel> GetByIdAsync(int id);
        Task<PostModel> CreateAsync(PostModel post);
        Task<bool> EditAsync(PostModel post);
        Task<bool> DeleteAsync(int id);
    }
    public class PostService : IPostService
    {
        private readonly MSPostContext _postContext;

        public PostService(MSPostContext context)
        {
            _postContext = context;
        }
        public async Task<IEnumerable<PostModel>> GetAllAsync()
        {
            return await _postContext.Posts.ToListAsync();
        }
        public async Task<IEnumerable<PostModel>> GetAllByUser(string userId)
        {
            return await _postContext.Posts.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<PostModel> GetByIdAsync(int id)
        {
            return await _postContext.Posts.FindAsync(id);
        }

        public async Task<PostModel> CreateAsync(PostModel post)
        {
            var result = await _postContext.Posts.AddAsync(post);
            int value = await _postContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<bool> EditAsync(PostModel post)
        {
            var candidate = await _postContext.Posts.FindAsync(post.Id);
            if(candidate is not null)
            {
                candidate.Title = post.Title;
                candidate.Body = post.Body;
                int value = await _postContext.SaveChangesAsync();
                return value > 0;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var candidate = await _postContext.Posts.FindAsync(id);
            if (candidate is not null)
            {
                _postContext.Posts.Remove(candidate);
                int value = await _postContext.SaveChangesAsync();
                return value > 0;
            }
            return false;
        }  
    }
}
