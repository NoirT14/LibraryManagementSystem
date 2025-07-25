using APIServer.DTO.Category;
using APIServer.DTO.CoverType;

namespace APIServer.Service.Interfaces
{
    public interface ICoverTypeService
    {
        IQueryable<CoverTypeResponse> GetAllAsQueryable();
        Task<CoverTypeResponse?> GetByIdAsync(int id);
        Task<CoverTypeResponse> CreateAsync(CoverTypeRequest dto);
        Task<bool> UpdateAsync(int id, CoverTypeRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
