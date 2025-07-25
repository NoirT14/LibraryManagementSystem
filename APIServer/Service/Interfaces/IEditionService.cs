using APIServer.DTO.CoverType;
using APIServer.DTO.Edition;

namespace APIServer.Service.Interfaces
{
    public interface IEditionService
    {
        IQueryable<EditionResponse> GetAllAsQueryable();
        Task<EditionResponse?> GetByIdAsync(int id);
        Task<EditionResponse> CreateAsync(EditionRequest dto);
        Task<bool> UpdateAsync(int id, EditionRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
