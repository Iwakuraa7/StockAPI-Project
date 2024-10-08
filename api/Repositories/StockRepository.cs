using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Interfaces;
using api.Models;
using api.Mappers;
using Microsoft.EntityFrameworkCore;
using api.Helpers;

namespace api.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _context;
        public StockRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Stock> CreateAsync(Stock stockModel)
        {
            await _context.AddAsync(stockModel);
            await _context.SaveChangesAsync();
            return stockModel;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            var stockToDelete = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);

            if(stockToDelete == null)
                return null;

            _context.Stocks.Remove(stockToDelete);
            await _context.SaveChangesAsync();
            return stockToDelete;
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.Stocks.AnyAsync(s => s.Id == id);
        }

        public async Task<Stock?> GetStockByIdAsync(int id)
        {
            return await _context.Stocks.Include(s => s.Comments).ThenInclude(a => a.AppUser).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Stock?> GetStockBySymbol(string symbol)
        {
            return await _context.Stocks.Include(s => s.Comments).FirstOrDefaultAsync(s => s.Symbol == symbol); 
        }

        public async Task<List<Stock>> GetStocksAsync(QueryObject query)
        {
            var stocks = _context.Stocks.Include(s => s.Comments).ThenInclude(a => a.AppUser).AsQueryable();

            if(!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));
            }

            if(!string.IsNullOrWhiteSpace(query.Symbol))
            {
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            }

            if(!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if(query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
                }
            }

            var skipNum = (query.PageNum - 1) * query.PageSize;

            return await stocks.Skip(skipNum).Take(query.PageSize).ToListAsync();
        }

        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            var stockToUpdate = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
            
            if(stockToUpdate == null)
                return null;

            stockToUpdate.Symbol = stockDto.Symbol;
            stockToUpdate.CompanyName = stockDto.CompanyName;
            stockToUpdate.Purchase = stockDto.Purchase;
            stockToUpdate.LastDiv = stockDto.LastDiv;
            stockToUpdate.Industry = stockDto.Industry;
            stockToUpdate.MarketCap = stockDto.MarketCap;

            await _context.SaveChangesAsync();
            
            return stockToUpdate;           
        }
    }
}