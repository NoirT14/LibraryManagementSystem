using APIServer.DTO.User;

namespace APIServer.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserReservationDTO?> GetUserByIdAsync(int userId);
    }
}
