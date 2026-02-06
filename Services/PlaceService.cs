using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly MetaplatformeContext _context;

        public PlaceService(MetaplatformeContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<PlaceResponse>>> GetPopularPlacesAsync(int limit = 6)
        {
            try
            {
                // Простой запрос без сложных включений
                var places = await _context.Places
                    .Take(limit)
                    .ToListAsync();

                var response = new List<PlaceResponse>();
                
                foreach (var place in places)
                {
                    var placeResponse = new PlaceResponse
                    {
                        Id = place.Id,
                        Name = place.Name ?? "Не указано",
                        Address = new AddressInfo
                        {
                            Id = 0,
                            City = "Город",
                            Street = "Улица",
                            House = "Дом"
                        },
                        Services = new List<ServiceInfo>(),
                        Equipments = new List<EquipmentInfo>(),
                        Characteristics = new List<CharacteristicInfo>(),
                        Photos = new List<PhotoInfo>()
                    };
                    
                    response.Add(placeResponse);
                }

                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при получении популярных площадок: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PlaceResponse>>> GetAllPlacesAsync(double? minRating = null)
        {
            try
            {
                var places = await _context.Places
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var response = new List<PlaceResponse>();
                
                foreach (var place in places)
                {
                    var placeResponse = new PlaceResponse
                    {
                        Id = place.Id,
                        Name = place.Name ?? "Не указано",
                        Address = new AddressInfo
                        {
                            Id = 0,
                            City = "Город",
                            Street = "Улица",
                            House = "Дом"
                        },
                        Services = new List<ServiceInfo>(),
                        Equipments = new List<EquipmentInfo>(),
                        Characteristics = new List<CharacteristicInfo>(),
                        Photos = new List<PhotoInfo>()
                    };
                    
                    response.Add(placeResponse);
                }

                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при получении мест: {ex.Message}");
            }
        }
        
        public async Task<ApiResponse<PlaceResponse>> GetPlaceByIdAsync(int id)
        {
            try
            {
                var place = await _context.Places
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                    return ApiResponse<PlaceResponse>.ErrorResponse("Площадка не найдена");

                var response = new PlaceResponse
                {
                    Id = place.Id,
                    Name = place.Name ?? "Не указано",
                    Address = new AddressInfo
                    {
                        Id = 0,
                        City = "Город",
                        Street = "Улица",
                        House = "Дом"
                    },
                    Services = new List<ServiceInfo>(),
                    Equipments = new List<EquipmentInfo>(),
                    Characteristics = new List<CharacteristicInfo>(),
                    Photos = new List<PhotoInfo>()
                };

                return ApiResponse<PlaceResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<PlaceResponse>.ErrorResponse($"Ошибка при получении площадки: {ex.Message}");
            }
        }
        
        public async Task<ApiResponse<List<PlaceResponse>>> SearchPlacesAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return ApiResponse<List<PlaceResponse>>.ErrorResponse("Поисковый запрос не может быть пустым");
                }

                var places = await _context.Places
                    .Where(p => p.Name != null && p.Name.Contains(term))
                    .Take(10)
                    .ToListAsync();

                var response = new List<PlaceResponse>();
                
                foreach (var place in places)
                {
                    var placeResponse = new PlaceResponse
                    {
                        Id = place.Id,
                        Name = place.Name ?? "Не указано",
                        Address = new AddressInfo
                        {
                            Id = 0,
                            City = "Город",
                            Street = "Улица",
                            House = "Дом"
                        },
                        Services = new List<ServiceInfo>(),
                        Equipments = new List<EquipmentInfo>(),
                        Characteristics = new List<CharacteristicInfo>(),
                        Photos = new List<PhotoInfo>()
                    };
                    
                    response.Add(placeResponse);
                }

                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при поиске мест: {ex.Message}");
            }
        }
        
        public async Task<ApiResponse<PlaceResponse>> CreatePlaceAsync(CreatePlaceRequest request)
        {
            return ApiResponse<PlaceResponse>.SuccessResponse(new PlaceResponse(), "Метод временно отключен");
        }
        
        public async Task<ApiResponse<PlaceResponse>> UpdatePlaceAsync(int id, UpdatePlaceRequest request)
        {
            return ApiResponse<PlaceResponse>.SuccessResponse(new PlaceResponse(), "Метод временно отключен");
        }
        
        public async Task<ApiResponse<bool>> DeletePlaceAsync(int id)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Метод временно отключен");
        }
    }
}
