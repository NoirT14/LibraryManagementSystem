using System.Threading.Tasks;
using APIServer.DTO.Reservation;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {
        Task CreateReservationAsync(int userId, int volumnId);
        Task CheckAvailableReservationsAsync(); // Gửi noti khi sách có sẵn
        Task ExpireOldReservationsAsync();      // Gửi noti khi hết hạn giữ
        Task<List<ReservationInfoListRespone>> GetReservationsByUserAsync(int userId);
    }
}
