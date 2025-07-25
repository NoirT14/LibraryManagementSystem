using APIServer.DTO.Edition;
using APIServer.DTO.PaperQuality;

namespace APIServer.Service.Interfaces
{
    public interface IPaperQualityService
    {
        IQueryable<PaperQualityResponse> GetAllAsQueryable();
        Task<PaperQualityResponse?> GetByIdAsync(int id);
        Task<PaperQualityResponse> CreateAsync(PaperQualityRequest dto);
        Task<bool> UpdateAsync(int id, PaperQualityRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
