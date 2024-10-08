using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IStockRepository _stockRepo;

        public StockController(ApplicationDBContext context, IStockRepository stockRepo)
        {
            _context = context;
            _stockRepo = stockRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetStocks([FromQuery] QueryObject query)
        {
            var stocks = await _stockRepo.GetStocksAsync(query);
            var stocksDto = stocks.Select(s => s.ToStockDto());

            return Ok(stocksDto);
        }

        [HttpGet("{stockId:int}")]
        public async Task<IActionResult> GetStockById([FromRoute] int stockId)
        {
            var stock = await _stockRepo.GetStockByIdAsync(stockId);

            if(stock == null)
                return NotFound();

            return Ok(stock.ToStockDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] CreateStockRequestDto stockDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = stockDto.ToStockFromCreateDto();
            await _stockRepo.CreateAsync(stockModel);

            return CreatedAtAction(nameof(GetStockById), new {stockId = stockModel.Id}, stockModel.ToStockDto());
        }

        [HttpPut]
        [Route("{stockId:int}")]
        public async Task<IActionResult> UpdateStock([FromRoute] int stockId, [FromBody] UpdateStockRequestDto stockDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockToUpdate = await _stockRepo.UpdateAsync(stockId, stockDto);

            if(stockToUpdate == null)
                return NotFound();

            return Ok(stockToUpdate.ToStockDto());
        }

        [HttpDelete]
        [Route("{stockId:int}")]
        public async Task<IActionResult> DeleteStock([FromRoute] int stockId)
        {
            var stockToDelete = await _stockRepo.DeleteAsync(stockId);

            if(stockToDelete == null)
                return NotFound();

            return NoContent();
        }
    }
}