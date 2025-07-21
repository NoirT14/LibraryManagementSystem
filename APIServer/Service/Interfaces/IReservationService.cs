using APIServer.Models;
using APIServer.DTO.Reservations;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(ReservationCreateDTO dto);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId);
    }
}
