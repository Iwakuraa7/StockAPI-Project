using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Comment;
using api.Dtos.Stock;
using api.Extensions;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(ICommentRepository commentRepo, IStockRepository stockRepo, UserManager<AppUser> userManager)
        {
            _commentRepo = commentRepo;
            _stockRepo = stockRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments() 
        {
            var comments = await _commentRepo.GetComments();

            return Ok(comments.Select(c => c.ToCommentDto()));
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            var comment = await _commentRepo.GetCommentByIdAsync(id);

            if(comment == null)
                return NotFound();

            return Ok(comment.ToCommentDto());
        }

        [HttpPost("{id:int}")]
        public async Task<IActionResult> Create([FromRoute] int id, [FromBody] CreateCommentDto commentDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!await _stockRepo.Exists(id))
                return NotFound("Stock doesn't exist.");

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);            

            var commentMap = commentDto.ToCommentFromCreateCommentDto(id);

            commentMap.AppUserId = appUser.Id;
            await _commentRepo.CreateAsync(commentMap);
            
            return CreatedAtAction(nameof(GetCommentById), new {id = commentMap.Id}, commentMap.ToCommentDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentDto commentDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var commentToUpdate = await _commentRepo.UpdateAsync(id, commentDto);

            if(commentToUpdate == null)
                return NotFound("Comment does not exist.");
            
            return Ok(commentToUpdate.ToCommentDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var commentToDelete = await _commentRepo.DeleteAsync(id);

            if(commentToDelete == null)
                return NotFound("Comment does not exist!");

            return NoContent();
        }
    }
}