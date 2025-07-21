using APIServer.Data;
using APIServer.DTO.Reservations;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class ReservationService : IReservationService
    {
        private readonly LibraryDatabaseContext _context;

        public ReservationService(LibraryDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Reservation> CreateReservationAsync(ReservationCreateDTO dto)
        {
            var reservation = new Reservation
            {
                UserId = dto.UserId,
                VariantId = dto.VariantId,
                ReservationStatus = "Pending",
                ReservationDate = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation == null || reservation.ReservationStatus != "Pending")
                return false;

            reservation.ReservationStatus = "Canceled";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Reservation>> GetUserReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }
    }
}
