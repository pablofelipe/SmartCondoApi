using SmartCondoApi.Dto;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services.User
{
    public interface IUserProfileService
    {
        Task<UserProfileResponseDTO> AddUserAsync(UserProfileCreateDTO userCreateDTO);

        Task<UserProfileResponseDTO> UpdateUserAsync(long userId, UserProfileUpdateDTO userUpdateDTO);

        Task<IEnumerable<UserProfile>> Get();

        Task<UserProfile> GetUser(long id);

        Task Delete(long id);
    }
}
