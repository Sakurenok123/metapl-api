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
                var places = await _context.Places
                    .Take(limit)
                    .Include(p => p.Address)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
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
                            Id = place.Address?.Id ?? 0,
                            City = place.Address?.City ?? "Не указано",
                            Street = place.Address?.Street ?? "Не указано",
                            House = place.Address?.House ?? "Не указано"
                        },
                        Services = place.ServicesPlaces?
                            .Select(sp => new ServiceInfo 
                            { 
                                Id = sp.Service?.Id ?? 0, 
                                Name = sp.Service?.Name ?? "Не указано" 
                            }).ToList() ?? new List<ServiceInfo>(),
                        Equipments = place.EquipmentsPlaces?
                            .Select(ep => new EquipmentInfo 
                            { 
                                Id = ep.Equipment?.Id ?? 0, 
                                Name = ep.Equipment?.Name ?? "Не указано" 
                            }).ToList() ?? new List<EquipmentInfo>(),
                        Characteristics = place.CharacteristicsPlaces?
                            .Select(cp => new CharacteristicInfo 
                            { 
                                Id = cp.Characteristic?.Id ?? 0, 
                                Name = cp.Characteristic?.Name ?? "Не указано" 
                            }).ToList() ?? new List<CharacteristicInfo>(),
                        Photos = place.PhotoPlaces?
                            .Select(pp => new PhotoInfo 
                            { 
                                Id = pp.Photo?.Id ?? 0, 
                                Url = pp.Photo?.Name ?? "" 
                            }).ToList() ?? new List<PhotoInfo>()
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
                    .Include(p => p.Address)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
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
                            Id = place.Address?.Id ?? 0,
                            City = place.Address?.City ?? "Не указано",
                            Street = place.Address?.Street ?? "Не указано",
                            House = place.Address?.House ?? "Не указано"
                        },
                        Services = place.ServicesPlaces?
                            .Select(sp => new ServiceInfo 
                            { 
                                Id = sp.Service?.Id ?? 0, 
                                Name = sp.Service?.Name ?? "Не указано" 
                            }).ToList() ?? new List<ServiceInfo>(),
                        Equipments = place.EquipmentsPlaces?
                            .Select(ep => new EquipmentInfo 
                            { 
                                Id = ep.Equipment?.Id ?? 0, 
                                Name = ep.Equipment?.Name ?? "Не указано" 
                            }).ToList() ?? new List<EquipmentInfo>(),
                        Characteristics = place.CharacteristicsPlaces?
                            .Select(cp => new CharacteristicInfo 
                            { 
                                Id = cp.Characteristic?.Id ?? 0, 
                                Name = cp.Characteristic?.Name ?? "Не указано" 
                            }).ToList() ?? new List<CharacteristicInfo>(),
                        Photos = place.PhotoPlaces?
                            .Select(pp => new PhotoInfo 
                            { 
                                Id = pp.Photo?.Id ?? 0, 
                                Url = pp.Photo?.Name ?? "" 
                            }).ToList() ?? new List<PhotoInfo>()
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
                    .Include(p => p.Address)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                    return ApiResponse<PlaceResponse>.ErrorResponse("Площадка не найдена");

                var response = new PlaceResponse
                {
                    Id = place.Id,
                    Name = place.Name ?? "Не указано",
                    Address = new AddressInfo
                    {
                        Id = place.Address?.Id ?? 0,
                        City = place.Address?.City ?? "Не указано",
                        Street = place.Address?.Street ?? "Не указано",
                        House = place.Address?.House ?? "Не указано"
                    },
                    Services = place.ServicesPlaces?
                        .Select(sp => new ServiceInfo 
                        { 
                            Id = sp.Service?.Id ?? 0, 
                            Name = sp.Service?.Name ?? "Не указано" 
                        }).ToList() ?? new List<ServiceInfo>(),
                    Equipments = place.EquipmentsPlaces?
                        .Select(ep => new EquipmentInfo 
                        { 
                            Id = ep.Equipment?.Id ?? 0, 
                            Name = ep.Equipment?.Name ?? "Не указано" 
                        }).ToList() ?? new List<EquipmentInfo>(),
                    Characteristics = place.CharacteristicsPlaces?
                        .Select(cp => new CharacteristicInfo 
                        { 
                            Id = cp.Characteristic?.Id ?? 0, 
                            Name = cp.Characteristic?.Name ?? "Не указано" 
                        }).ToList() ?? new List<CharacteristicInfo>(),
                    Photos = place.PhotoPlaces?
                        .Select(pp => new PhotoInfo 
                        { 
                            Id = pp.Photo?.Id ?? 0, 
                            Url = pp.Photo?.Name ?? "" 
                        }).ToList() ?? new List<PhotoInfo>()
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
                    .Include(p => p.Address)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
                    .Where(p => p.Name != null && p.Name.Contains(term) ||
                                (p.Address != null && p.Address.City != null && p.Address.City.Contains(term)) ||
                                (p.Address != null && p.Address.Street != null && p.Address.Street.Contains(term)))
                    .OrderBy(p => p.Name)
                    .Take(50)
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
                            Id = place.Address?.Id ?? 0,
                            City = place.Address?.City ?? "Не указано",
                            Street = place.Address?.Street ?? "Не указано",
                            House = place.Address?.House ?? "Не указано"
                        },
                        Services = place.ServicesPlaces?
                            .Select(sp => new ServiceInfo 
                            { 
                                Id = sp.Service?.Id ?? 0, 
                                Name = sp.Service?.Name ?? "Не указано" 
                            }).ToList() ?? new List<ServiceInfo>(),
                        Equipments = place.EquipmentsPlaces?
                            .Select(ep => new EquipmentInfo 
                            { 
                                Id = ep.Equipment?.Id ?? 0, 
                                Name = ep.Equipment?.Name ?? "Не указано" 
                            }).ToList() ?? new List<EquipmentInfo>(),
                        Characteristics = place.CharacteristicsPlaces?
                            .Select(cp => new CharacteristicInfo 
                            { 
                                Id = cp.Characteristic?.Id ?? 0, 
                                Name = cp.Characteristic?.Name ?? "Не указано" 
                            }).ToList() ?? new List<CharacteristicInfo>(),
                        Photos = place.PhotoPlaces?
                            .Select(pp => new PhotoInfo 
                            { 
                                Id = pp.Photo?.Id ?? 0, 
                                Url = pp.Photo?.Name ?? "" 
                            }).ToList() ?? new List<PhotoInfo>()
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
            return ApiResponse<PlaceResponse>.ErrorResponse("Метод временно отключен");
        }
        
        public async Task<ApiResponse<PlaceResponse>> UpdatePlaceAsync(int id, UpdatePlaceRequest request)
        {
            return ApiResponse<PlaceResponse>.ErrorResponse("Метод временно отключен");
        }
        
        public async Task<ApiResponse<bool>> DeletePlaceAsync(int id)
        {
            return ApiResponse<bool>.ErrorResponse("Метод временно отключен");
        }
    }
}
