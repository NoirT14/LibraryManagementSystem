using System.Threading.Tasks;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {
        Task CreateReservationAsync(int userId, int variantId);
        Task CheckAvailableReservationsAsync(); // Gửi noti khi sách có sẵn
        Task ExpireOldReservationsAsync();      // Gửi noti khi hết hạn giữ
    }
}
