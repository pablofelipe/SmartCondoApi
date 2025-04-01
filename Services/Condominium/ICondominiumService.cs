using SmartCondoApi.Dto;

namespace SmartCondoApi.Services.Condominium
{
    public interface ICondominiumService
    {
        Task<IEnumerable<Models.Condominium>> Get();

        Task<Models.Condominium> Get(int condominiumId);

        Task<List<UserProfileResponseDTO>> SearchUsers(int condominiumId, UserProfileSearchDTO searchDto);
    }
}
