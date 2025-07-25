using APIServer.Data;
using APIServer.DTO.Author;
using APIServer.DTO.Category;
using APIServer.Models;
using APIServer.Service.Interfaces;
using APIServer.util;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly LibraryDatabaseContext _context;

        public CategoryService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<CategoryResponse> GetAllAsQueryable()
        {
            return _context.Categories
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    BookCount = c.Books.Count()
                });
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
            if (string.IsNullOrEmpty(dto.CategoryName))
            {
                throw new InvalidOperationException("Category is empty.");
            }

            if (StringHelper.ExistsInList(dto.CategoryName, _context.Categories.Select(c => c.CategoryName).ToList())) throw new InvalidOperationException("Category already exists.");

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
            if (string.IsNullOrEmpty(dto.CategoryName))
            {
                throw new InvalidOperationException("Category is empty.");
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null) return false;

            if(StringHelper.ExistsInList(dto.CategoryName, _context.Categories.Select(c => c.CategoryName).ToList())) return false;

            category.CategoryName = dto.CategoryName;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsDelete = true;
            _context.Categories.Update(category);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}