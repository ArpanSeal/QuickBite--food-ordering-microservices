using ProjectSolution.Web.Models;

namespace ProjectSolution.Web.Service.IService
{
    public interface IBaseService
    {
        Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearerToken = true);
    }
}
