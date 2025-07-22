using APIServer.DTO.Category;

namespace APIServer.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse?> GetByIdAsync(int id);
        Task<CategoryResponse> CreateAsync(CategoryRequest dto);
        Task<bool> UpdateAsync(int id, CategoryRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
