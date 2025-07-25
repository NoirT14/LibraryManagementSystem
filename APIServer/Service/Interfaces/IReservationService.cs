using APIServer.Models;
using APIServer.DTO.Reservations;
using APIServer.DTO.Reservation;

namespace APIServer.Service.Interfaces
{
    public interface IReservationService
    {//
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
        Task<List<BookAvailabilityDTO>> GetBookAvailabilityByBookIdAsync(int bookId);

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

        Task CheckAvailableReservationsAsync(); // Gửi noti khi sách có sẵn
        Task ExpireOldReservationsAsync();      // Gửi noti khi hết hạn giữ

        //The
        Task<int> CountReservationsAsync();
        Task<int> CountByStatusAsync(string status);
    }
}
