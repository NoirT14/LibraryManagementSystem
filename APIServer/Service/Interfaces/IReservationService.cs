using APIServer.Models;
using APIServer.DTO.Reservations;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation?> CreateReservationAsync(ReservationCreateDTO dto);
        Task<bool> CancelReservationAsync(int reservationId, int userId);
        Task<bool> CancelReservationByStaffAsync(int reservationId, int staffId);

        // User operations
        Task<List<ReservationListDTO>> GetUserReservationsAsync(int userId);

        // Staff operations - Queue management
        IQueryable<ReservationListDTO> GetAllReservations();
        Task<int> GetReservationsCountAsync(string? keyword);
        Task<ReservationListDTO?> GetReservationByIdAsync(int reservationId);
        Task<bool> UpdateReservationAsync(int reservationId, ReservationUpdateDTO dto);

        // Availability check
        Task<BookAvailabilityDTO?> GetBookAvailabilityAsync(int variantId);

        // Queue operations
        Task<int> GetQueuePositionAsync(int reservationId);
        Task<List<ReservationListDTO>> GetReservationQueueAsync(int variantId);

        // Auto-processing
        Task ProcessExpiredReservationsAsync();
        Task<Reservation?> GetNextAvailableReservationAsync(int variantId);
        Task<List<ReservationListDTO>> GetAllReservationsListAsync(
    int page = 1, int pageSize = 10, string? keyword = null, string? status = null);
        Task NotifyNextReservationAsync(int variantId);
        Task<bool> HasPendingReservationsAsync(int variantId);
    }
}
