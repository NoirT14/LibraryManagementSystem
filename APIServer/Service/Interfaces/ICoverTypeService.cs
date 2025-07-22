using APIServer.DTO.CoverType;

namespace APIServer.Service.Interfaces
{
    public interface ICoverTypeService
    {
        Task<IEnumerable<CoverTypeResponse>> GetAllAsync();
        Task<CoverTypeResponse?> GetByIdAsync(int id);
        Task<CoverTypeResponse> CreateAsync(CoverTypeRequest dto);
        Task<bool> UpdateAsync(int id, CoverTypeRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
