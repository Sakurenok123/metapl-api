using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Services
{
    public interface IPlaceService
    {
        Task<ApiResponse<List<PlaceResponse>>> GetAllPlacesAsync(double? minRating = null);
        Task<ApiResponse<List<PlaceResponse>>> GetPopularPlacesAsync(int limit = 6);
        Task<ApiResponse<PlaceResponse>> GetPlaceByIdAsync(int id);
        Task<ApiResponse<List<PlaceResponse>>> SearchPlacesAsync(string term);
        Task<ApiResponse<PlaceResponse>> CreatePlaceAsync(CreatePlaceRequest request);
        Task<ApiResponse<PlaceResponse>> UpdatePlaceAsync(int id, UpdatePlaceRequest request);
        Task<ApiResponse<bool>> DeletePlaceAsync(int id);
    }
}
