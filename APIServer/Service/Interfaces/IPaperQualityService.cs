using APIServer.DTO.PaperQuality;

namespace APIServer.Service.Interfaces
{
    public interface IPaperQualityService
    {
        Task<IEnumerable<PaperQualityResponse>> GetAllAsync();
        Task<PaperQualityResponse?> GetByIdAsync(int id);
        Task<PaperQualityResponse> CreateAsync(PaperQualityRequest dto);
        Task<bool> UpdateAsync(int id, PaperQualityRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
