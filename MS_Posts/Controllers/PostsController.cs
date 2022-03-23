using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MS_Posts.Models;
using MS_Posts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MS_Posts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            this._postService = postService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var postList = await _postService.GetAllAsync();
            return Ok(postList);
        }
        [AllowAnonymous]
        [HttpGet("getbyuser/{id}")]
        public async Task<IActionResult> GetAllByUser(string userId)
        {
            var postList = await _postService.GetAllByUser(userId);
            return Ok(postList);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentPost = await _postService.GetByIdAsync(id);
            if (currentPost is not null)
            {
                return Ok(currentPost);
            }
            return BadRequest(new { message = "post not found" });
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(PostModel post)
        {
            var result = await _postService.CreateAsync(post);
            if (result is not null)
            {
                return Created(result.Id.ToString(),result);
            }
            return BadRequest(new { message = "Model invalid" });
        }
        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> Edit(PostModel post)
        {
            var result = await _postService.EditAsync(post);
            if (result)
            {
                return Created(post.Id.ToString(), result);
            }
            return BadRequest(new { message = "post not found" });
        }
        [AllowAnonymous]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await _postService.DeleteAsync(id);
            if (result)
            {
                return NoContent();
            }
            return BadRequest(new { message = "post not found" });
        }
    }
}
