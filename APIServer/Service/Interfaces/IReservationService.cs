using System.Threading.Tasks;
using APIServer.DTO.Reservation;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {
        Task CreateReservationAsync(int userId, int volumnId);
        Task CheckAvailableReservationsAsync(); // Gửi noti khi sách có sẵn
        Task ExpireOldReservationsAsync();      // Gửi noti khi hết hạn giữ

        //The
        Task<int> CountReservationsAsync();
        Task<int> CountByStatusAsync(string status);
        Task<List<ReservationInfoListRespone>> GetReservationsByUserAsync(int userId);
    }
}
