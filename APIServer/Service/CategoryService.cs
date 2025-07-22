using APIServer.Data;
using APIServer.DTO.Category;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace APIServer.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly LibraryDatabaseContext _context;

        public CategoryService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    BookCount = c.Books.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<CategoryResponse> CreateAsync(CategoryRequest dto)
        {
            var category = new Category
            {
                CategoryName = dto.CategoryName
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName
            };
        }

        public async Task<bool> UpdateAsync(int id, CategoryRequest dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.CategoryName = dto.CategoryName;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}