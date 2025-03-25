using SmartCondoApi.Dto;

namespace SmartCondoApi.Services.Condominium
{
    public interface ICondominiumService
    {
        Task<IEnumerable<Models.Condominium>> Get();

        Task<List<UserProfileResponseDTO>> SearchUsers(int condominiumId, UserProfileSearchDTO searchDto);
    }
}
