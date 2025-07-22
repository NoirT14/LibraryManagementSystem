using APIServer.DTO.Publisher;

namespace APIServer.Service.Interfaces
{
    public interface IPublisherService
    {
        Task<IEnumerable<PublisherResponse>> GetAllAsync();
        Task<PublisherResponse?> GetByIdAsync(int id);
        Task<PublisherResponse> CreateAsync(PublisherRequest dto);
        Task<bool> UpdateAsync(int id, PublisherRequest dto);
        Task<bool> DeleteAsync(int id);
    }
}
