using APIServer.DTO.PaperQuality;
using APIServer.DTO.Publisher;

namespace APIServer.Service.Interfaces
{
    public interface IPublisherService
    {
        IQueryable<PublisherResponse> GetAllAsQueryable();
        Task<PublisherResponse?> GetByIdAsync(int id);
        Task<PublisherResponse> CreateAsync(PublisherRequest dto);
        Task<bool> UpdateAsync(int id, PublisherRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
