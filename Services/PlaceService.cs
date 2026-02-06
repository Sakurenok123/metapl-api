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
                Console.WriteLine($"Getting popular places, limit: {limit}");
                
                // Получаем места с самым большим количеством заявок
                var popularPlaceIds = await _context.Applications
                    .Where(a => a.PlaceId != null)
                    .GroupBy(a => a.PlaceId)
                    .Select(g => new { PlaceId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.PlaceId)
                    .Take(limit)
                    .ToListAsync();

                Console.WriteLine($"Popular place IDs: {string.Join(", ", popularPlaceIds)}");

                // Если нет популярных по заявкам, берем просто первые места
                if (!popularPlaceIds.Any())
                {
                    popularPlaceIds = await _context.Places
                        .OrderBy(p => p.Id)
                        .Select(p => p.Id)
                        .Take(limit)
                        .ToListAsync();
                }

                var places = await _context.Places
                    .Where(p => popularPlaceIds.Contains(p.Id))
                    .Include(p => p.Address)
                    .Include(p => p.EquipmentsPlaces)
                        .ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces)
                        .ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.ServicesPlaces)
                        .ThenInclude(sp => sp.Service)
                    .Include(p => p.PhotoPlaces)
                        .ThenInclude(pp => pp.Photo)
                    .ToListAsync();

                Console.WriteLine($"Found {places.Count} places");

                // Запрашиваем рейтинги
                var placeIds = places.Select(p => p.Id).ToList();
                var reviewStats = await _context.PlaceReviews
                    .Where(r => placeIds.Contains(r.PlaceId))
                    .GroupBy(r => r.PlaceId)
                    .Select(g => new 
                    { 
                        PlaceId = g.Key, 
                        Avg = g.Average(r => r.Rating), 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                var statsDict = reviewStats.ToDictionary(s => s.PlaceId);

                var response = places.Select(p =>
                {
                    var stats = statsDict.GetValueOrDefault(p.Id);
                    return new PlaceResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Address = p.Address != null ? new AddressInfo
                        {
                            Id = p.Address.Id,
                            City = p.Address.City ?? "Не указано",
                            Street = p.Address.Street ?? "Не указано",
                            House = p.Address.House ?? "Не указано"
                        } : new AddressInfo
                        {
                            Id = 0,
                            City = "Не указано",
                            Street = "Не указано",
                            House = "Не указано"
                        },
                        Equipments = p.EquipmentsPlaces?
                            .Select(ep => new EquipmentInfo 
                            { 
                                Id = ep.Equipment?.Id ?? 0, 
                                Name = ep.Equipment?.Name ?? "Не указано" 
                            }).ToList() ?? new List<EquipmentInfo>(),
                        Characteristics = p.CharacteristicsPlaces?
                            .Select(cp => new CharacteristicInfo 
                            { 
                                Id = cp.Characteristic?.Id ?? 0, 
                                Name = cp.Characteristic?.Name ?? "Не указано" 
                            }).ToList() ?? new List<CharacteristicInfo>(),
                        Services = p.ServicesPlaces?
                            .Select(sp => new ServiceInfo 
                            { 
                                Id = sp.Service?.Id ?? 0, 
                                Name = sp.Service?.Name ?? "Не указано" 
                            }).ToList() ?? new List<ServiceInfo>(),
                        Photos = p.PhotoPlaces?
                            .Select(pp => new PhotoInfo 
                            { 
                                Id = pp.Photo?.Id ?? 0, 
                                Url = pp.Photo?.Name ?? "" 
                            }).ToList() ?? new List<PhotoInfo>(),
                        AverageRating = stats != null ? Math.Round(stats.Avg, 1) : (double?)null,
                        ReviewCount = stats?.Count ?? 0
                    };
                }).ToList();

                Console.WriteLine($"Returning {response.Count} popular places");
                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetPopularPlacesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при получении популярных площадок: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PlaceResponse>>> GetAllPlacesAsync(double? minRating = null)
        {
            try
            {
                Console.WriteLine("Getting all places");
                
                var places = await _context.Places
                    .Include(p => p.Address)
                    .Include(p => p.EquipmentsPlaces)
                        .ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces)
                        .ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.ServicesPlaces)
                        .ThenInclude(sp => sp.Service)
                    .Include(p => p.PhotoPlaces)
                        .ThenInclude(pp => pp.Photo)
                    .OrderBy(p => p.Id)
                    .ToListAsync();

                Console.WriteLine($"Found {places.Count} places total");

                var placeIds = places.Select(p => p.Id).ToList();
                var reviewStats = await _context.PlaceReviews
                    .Where(r => placeIds.Contains(r.PlaceId))
                    .GroupBy(r => r.PlaceId)
                    .Select(g => new 
                    { 
                        PlaceId = g.Key, 
                        Avg = g.Average(r => r.Rating), 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                var statsDict = reviewStats.ToDictionary(s => s.PlaceId);

                var response = places.Select(p =>
                {
                    var stats = statsDict.GetValueOrDefault(p.Id);
                    var avg = stats != null ? Math.Round(stats.Avg, 1) : (double?)null;
                    var count = stats?.Count ?? 0;
                    
                    // Фильтр по рейтингу
                    if (minRating.HasValue && minRating.Value >= 1 && minRating.Value <= 5)
                    {
                        if (!avg.HasValue || avg.Value < minRating.Value)
                            return null;
                    }

                    return new PlaceResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Address = p.Address != null ? new AddressInfo
                        {
                            Id = p.Address.Id,
                            City = p.Address.City ?? "Не указано",
                            Street = p.Address.Street ?? "Не указано",
                            House = p.Address.House ?? "Не указано"
                        } : new AddressInfo
                        {
                            Id = 0,
                            City = "Не указано",
                            Street = "Не указано",
                            House = "Не указано"
                        },
                        Equipments = p.EquipmentsPlaces?
                            .Select(ep => new EquipmentInfo 
                            { 
                                Id = ep.Equipment?.Id ?? 0, 
                                Name = ep.Equipment?.Name ?? "Не указано" 
                            }).ToList() ?? new List<EquipmentInfo>(),
                        Characteristics = p.CharacteristicsPlaces?
                            .Select(cp => new CharacteristicInfo 
                            { 
                                Id = cp.Characteristic?.Id ?? 0, 
                                Name = cp.Characteristic?.Name ?? "Не указано" 
                            }).ToList() ?? new List<CharacteristicInfo>(),
                        Services = p.ServicesPlaces?
                            .Select(sp => new ServiceInfo 
                            { 
                                Id = sp.Service?.Id ?? 0, 
                                Name = sp.Service?.Name ?? "Не указано" 
                            }).ToList() ?? new List<ServiceInfo>(),
                        Photos = p.PhotoPlaces?
                            .Select(pp => new PhotoInfo 
                            { 
                                Id = pp.Photo?.Id ?? 0, 
                                Url = pp.Photo?.Name ?? "" 
                            }).ToList() ?? new List<PhotoInfo>(),
                        AverageRating = avg,
                        ReviewCount = count
                    };
                }).Where(p => p != null).ToList();

                Console.WriteLine($"Returning {response.Count} places after filtering");
                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetAllPlacesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при получении мест: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlaceResponse>> GetPlaceByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"Getting place by ID: {id}");
                
                var place = await _context.Places
                    .Include(p => p.Address)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                {
                    Console.WriteLine($"Place with ID {id} not found");
                    return ApiResponse<PlaceResponse>.ErrorResponse("Площадка не найдена");
                }

                Console.WriteLine($"Found place: {place.Name}");

                // Получаем статистику отзывов
                var reviewStats = await _context.PlaceReviews
                    .Where(r => r.PlaceId == id)
                    .GroupBy(r => r.PlaceId)
                    .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
                    .FirstOrDefaultAsync();

                var response = new PlaceResponse
                {
                    Id = place.Id,
                    Name = place.Name,
                    Address = place.Address != null ? new AddressInfo
                    {
                        Id = place.Address.Id,
                        City = place.Address.City ?? "Не указано",
                        Street = place.Address.Street ?? "Не указано",
                        House = place.Address.House ?? "Не указано"
                    } : new AddressInfo
                    {
                        Id = 0,
                        City = "Не указано",
                        Street = "Не указано",
                        House = "Не указано"
                    },
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
                    Services = place.ServicesPlaces?
                        .Select(sp => new ServiceInfo
                        {
                            Id = sp.Service?.Id ?? 0,
                            Name = sp.Service?.Name ?? "Не указано"
                        }).ToList() ?? new List<ServiceInfo>(),
                    Photos = place.PhotoPlaces?
                        .Select(pp => new PhotoInfo
                        {
                            Id = pp.Photo?.Id ?? 0,
                            Url = pp.Photo?.Name ?? ""
                        }).ToList() ?? new List<PhotoInfo>(),
                };

                if (reviewStats != null) 
                { 
                    response.AverageRating = Math.Round(reviewStats.Avg, 1); 
                    response.ReviewCount = reviewStats.Count; 
                }

                Console.WriteLine($"Successfully returned place with ID {id}");
                return ApiResponse<PlaceResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetPlaceByIdAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<PlaceResponse>.ErrorResponse($"Ошибка при получении площадки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PlaceResponse>>> SearchPlacesAsync(string term)
        {
            try
            {
                Console.WriteLine($"Searching places with term: {term}");
                
                if (string.IsNullOrWhiteSpace(term))
                {
                    return ApiResponse<List<PlaceResponse>>.ErrorResponse("Поисковый запрос не может быть пустым");
                }

                var places = await _context.Places
                    .Include(p => p.Address)
                    .Include(p => p.EquipmentsPlaces).ThenInclude(ep => ep.Equipment)
                    .Include(p => p.CharacteristicsPlaces).ThenInclude(cp => cp.Characteristic)
                    .Include(p => p.ServicesPlaces).ThenInclude(sp => sp.Service)
                    .Include(p => p.PhotoPlaces).ThenInclude(pp => pp.Photo)
                    .Where(p => p.Name.Contains(term) ||
                                (p.Address != null && (
                                    p.Address.City.Contains(term) ||
                                    p.Address.Street.Contains(term))) ||
                                p.EquipmentsPlaces.Any(ep => ep.Equipment != null && ep.Equipment.Name.Contains(term)) ||
                                p.CharacteristicsPlaces.Any(cp => cp.Characteristic != null && cp.Characteristic.Name.Contains(term)) ||
                                p.ServicesPlaces.Any(sp => sp.Service != null && sp.Service.Name.Contains(term)))
                    .OrderBy(p => p.Address != null ? p.Address.City : "")
                    .ThenBy(p => p.Address != null ? p.Address.Street : "")
                    .Take(50)
                    .ToListAsync();

                Console.WriteLine($"Found {places.Count} places matching search");

                var response = places.Select(p => new PlaceResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address != null ? new AddressInfo
                    {
                        Id = p.Address.Id,
                        City = p.Address.City ?? "Не указано",
                        Street = p.Address.Street ?? "Не указано",
                        House = p.Address.House ?? "Не указано"
                    } : new AddressInfo
                    {
                        Id = 0,
                        City = "Не указано",
                        Street = "Не указано",
                        House = "Не указано"
                    },
                    Equipments = p.EquipmentsPlaces?
                        .Select(ep => new EquipmentInfo
                        {
                            Id = ep.Equipment?.Id ?? 0,
                            Name = ep.Equipment?.Name ?? "Не указано"
                        }).ToList() ?? new List<EquipmentInfo>(),
                    Characteristics = p.CharacteristicsPlaces?
                        .Select(cp => new CharacteristicInfo
                        {
                            Id = cp.Characteristic?.Id ?? 0,
                            Name = cp.Characteristic?.Name ?? "Не указано"
                        }).ToList() ?? new List<CharacteristicInfo>(),
                    Services = p.ServicesPlaces?
                        .Select(sp => new ServiceInfo
                        {
                            Id = sp.Service?.Id ?? 0,
                            Name = sp.Service?.Name ?? "Не указано"
                        }).ToList() ?? new List<ServiceInfo>(),
                    Photos = p.PhotoPlaces?
                        .Select(pp => new PhotoInfo
                        {
                            Id = pp.Photo?.Id ?? 0,
                            Url = pp.Photo?.Name ?? ""
                        }).ToList() ?? new List<PhotoInfo>()
                }).ToList();

                return ApiResponse<List<PlaceResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in SearchPlacesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse($"Ошибка при поиске мест: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlaceResponse>> CreatePlaceAsync(CreatePlaceRequest request)
        {
            try
            {
                Console.WriteLine($"Creating place: {request.Name}");
                
                // Проверка существования адреса
                var address = await _context.Addresses.FindAsync(request.AddressesId);
                if (address == null) 
                    return ApiResponse<PlaceResponse>.ErrorResponse("Указанный адрес не существует");

                // Создание площадки
                var place = new Place
                {
                    Name = request.Name,
                    AddressId = request.AddressesId
                };
                await _context.Places.AddAsync(place);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Place created with ID: {place.Id}");

                // Добавление оборудования (несколько)
                if (request.EquipmentsIds != null)
                {
                    foreach (var eqId in request.EquipmentsIds.Distinct())
                    {
                        var equipment = await _context.Equipments.FindAsync(eqId);
                        if (equipment != null)
                            await _context.EquipmentsPlaces.AddAsync(new EquipmentsPlace { PlaceId = place.Id, EquipmentId = eqId });
                    }
                }

                // Добавление характеристик (несколько)
                if (request.CharacteristicsIds != null)
                {
                    foreach (var chId in request.CharacteristicsIds.Distinct())
                    {
                        var characteristic = await _context.Characteristics.FindAsync(chId);
                        if (characteristic != null)
                            await _context.CharacteristicsPlaces.AddAsync(new CharacteristicsPlace { PlaceId = place.Id, CharacteristicId = chId });
                    }
                }

                // Добавление услуг (несколько)
                if (request.ServiceIds != null)
                {
                    foreach (var svcId in request.ServiceIds.Distinct())
                    {
                        var service = await _context.Services.FindAsync(svcId);
                        if (service != null)
                            await _context.ServicesPlaces.AddAsync(new ServicesPlace { PlaceId = place.Id, ServiceId = svcId });
                    }
                }

                // Добавление фотографий
                if (request.PhotoIds != null && request.PhotoIds.Any())
                {
                    foreach (var photoId in request.PhotoIds)
                    {
                        var photo = await _context.Photos.FindAsync(photoId);
                        if (photo != null)
                        {
                            await _context.PhotoPlaces.AddAsync(new PhotoPlace
                            {
                                PlaceId = place.Id,
                                PhotoId = photoId,
                                IsMain = request.PhotoIds.First() == photoId
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Получаем созданную площадку с полными данными
                var createdPlace = await GetPlaceByIdAsync(place.Id);
                Console.WriteLine($"Place creation completed successfully");
                return createdPlace;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreatePlaceAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<PlaceResponse>.ErrorResponse($"Ошибка при создании площадки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlaceResponse>> UpdatePlaceAsync(int id, UpdatePlaceRequest request)
        {
            try
            {
                Console.WriteLine($"Updating place ID: {id}");
                
                var place = await _context.Places.FindAsync(id);
                if (place == null)
                {
                    Console.WriteLine($"Place with ID {id} not found");
                    return ApiResponse<PlaceResponse>.ErrorResponse("Место не найдено");
                }

                // Обновляем основные поля
                if (!string.IsNullOrEmpty(request.Name))
                {
                    place.Name = request.Name;
                }

                if (request.AddressesId.HasValue)
                {
                    var address = await _context.Addresses.FindAsync(request.AddressesId.Value);
                    if (address == null)
                    {
                        return ApiResponse<PlaceResponse>.ErrorResponse("Указанный адрес не существует");
                    }
                    place.AddressId = request.AddressesId.Value;
                }

                await _context.SaveChangesAsync();

                // Обновляем связи через junction-таблицы (списки заменяют текущие)
                if (request.EquipmentsIds != null)
                {
                    var existing = await _context.EquipmentsPlaces.Where(ep => ep.PlaceId == id).ToListAsync();
                    _context.EquipmentsPlaces.RemoveRange(existing);
                    foreach (var eqId in request.EquipmentsIds.Distinct())
                    {
                        var equipment = await _context.Equipments.FindAsync(eqId);
                        if (equipment != null)
                            await _context.EquipmentsPlaces.AddAsync(new EquipmentsPlace { PlaceId = id, EquipmentId = eqId });
                    }
                }

                if (request.CharacteristicsIds != null)
                {
                    var existing = await _context.CharacteristicsPlaces.Where(cp => cp.PlaceId == id).ToListAsync();
                    _context.CharacteristicsPlaces.RemoveRange(existing);
                    foreach (var chId in request.CharacteristicsIds.Distinct())
                    {
                        var characteristic = await _context.Characteristics.FindAsync(chId);
                        if (characteristic != null)
                            await _context.CharacteristicsPlaces.AddAsync(new CharacteristicsPlace { PlaceId = id, CharacteristicId = chId });
                    }
                }

                if (request.ServiceIds != null)
                {
                    var existing = await _context.ServicesPlaces.Where(sp => sp.PlaceId == id).ToListAsync();
                    _context.ServicesPlaces.RemoveRange(existing);
                    foreach (var svcId in request.ServiceIds.Distinct())
                    {
                        var service = await _context.Services.FindAsync(svcId);
                        if (service != null)
                            await _context.ServicesPlaces.AddAsync(new ServicesPlace { PlaceId = id, ServiceId = svcId });
                    }
                }

                // ОБНОВЛЕНИЕ ФОТОГРАФИЙ
                if (request.PhotoIds != null && request.PhotoIds.Any())
                {
                    // Получаем существующие связи фото с местом
                    var existingPhotoPlaces = await _context.PhotoPlaces
                        .Where(pp => pp.PlaceId == id)
                        .ToListAsync();

                    // Получаем существующие фото
                    var existingPhotos = await _context.Photos
                        .Where(p => request.PhotoIds.Contains(p.Id))
                        .ToListAsync();

                    // Проверяем, что все переданные фото существуют
                    if (existingPhotos.Count != request.PhotoIds.Count)
                    {
                        return ApiResponse<PlaceResponse>.ErrorResponse("Одно или несколько фото не найдены");
                    }

                    // Определяем, какие связи нужно удалить
                    var photosToRemove = existingPhotoPlaces
                        .Where(ep => !request.PhotoIds.Contains(ep.PhotoId))
                        .ToList();

                    // Удаляем старые связи
                    if (photosToRemove.Any())
                    {
                        _context.PhotoPlaces.RemoveRange(photosToRemove);
                    }

                    // Определяем, какие связи нужно добавить
                    var existingPhotoIds = existingPhotoPlaces.Select(ep => ep.PhotoId).ToList();
                    var photosToAdd = request.PhotoIds
                        .Where(photoId => !existingPhotoIds.Contains(photoId))
                        .ToList();

                    // Добавляем новые связи
                    foreach (var photoId in photosToAdd)
                    {
                        // Определяем, какое фото будет главным (первое в списке)
                        bool isMain = request.PhotoIds.First() == photoId;

                        // Если добавляем главное фото, снимаем флаг главного с других
                        if (isMain && existingPhotoPlaces.Any())
                        {
                            foreach (var existingPhoto in existingPhotoPlaces)
                            {
                                existingPhoto.IsMain = false;
                            }
                        }

                        await _context.PhotoPlaces.AddAsync(new PhotoPlace
                        {
                            PlaceId = id,
                            PhotoId = photoId,
                            IsMain = isMain
                        });
                    }

                    // Обновляем флаг IsMain для существующих фото, если нужно
                    if (request.PhotoIds.Any() && existingPhotoPlaces.Any())
                    {
                        var firstPhotoId = request.PhotoIds.First();
                        var mainPhotoPlace = existingPhotoPlaces.FirstOrDefault(pp => pp.PhotoId == firstPhotoId);
                        if (mainPhotoPlace != null)
                        {
                            // Снимаем флаг главного с других фото
                            foreach (var photoPlace in existingPhotoPlaces)
                            {
                                photoPlace.IsMain = false;
                            }
                            // Устанавливаем флаг главного для первого фото
                            mainPhotoPlace.IsMain = true;
                        }
                    }
                }
                else if (request.PhotoIds != null && !request.PhotoIds.Any())
                {
                    // Если передан пустой список фото, удаляем все связи
                    var existingPhotoPlaces = await _context.PhotoPlaces
                        .Where(pp => pp.PlaceId == id)
                        .ToListAsync();

                    if (existingPhotoPlaces.Any())
                    {
                        _context.PhotoPlaces.RemoveRange(existingPhotoPlaces);
                    }
                }

                await _context.SaveChangesAsync();

                // Получаем обновленное место со всеми данными
                var updatedPlace = await GetPlaceByIdAsync(id);
                Console.WriteLine($"Place update completed successfully");
                return updatedPlace;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UpdatePlaceAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<PlaceResponse>.ErrorResponse($"Ошибка при обновлении места: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePlaceAsync(int id)
        {
            try
            {
                Console.WriteLine($"Deleting place ID: {id}");
                
                var place = await _context.Places
                    .Include(p => p.Applications)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (place == null)
                {
                    Console.WriteLine($"Place with ID {id} not found");
                    return ApiResponse<bool>.ErrorResponse("Место не найдено");
                }

                // Проверяем, есть ли связанные заявки
                if (place.Applications.Any())
                {
                    Console.WriteLine($"Cannot delete place {id} - has {place.Applications.Count} applications");
                    return ApiResponse<bool>.ErrorResponse("Невозможно удалить место, так как к нему привязаны заявки");
                }

                // Удаляем связанные записи из junction-таблиц
                var equipmentsPlaces = await _context.EquipmentsPlaces.Where(ep => ep.PlaceId == id).ToListAsync();
                var characteristicsPlaces = await _context.CharacteristicsPlaces.Where(cp => cp.PlaceId == id).ToListAsync();
                var servicesPlaces = await _context.ServicesPlaces.Where(sp => sp.PlaceId == id).ToListAsync();
                var photoPlaces = await _context.PhotoPlaces.Where(pp => pp.PlaceId == id).ToListAsync();

                _context.EquipmentsPlaces.RemoveRange(equipmentsPlaces);
                _context.CharacteristicsPlaces.RemoveRange(characteristicsPlaces);
                _context.ServicesPlaces.RemoveRange(servicesPlaces);
                _context.PhotoPlaces.RemoveRange(photoPlaces);

                _context.Places.Remove(place);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Place {id} deleted successfully");
                return ApiResponse<bool>.SuccessResponse(true, "Место успешно удалено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in DeletePlaceAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResponse<bool>.ErrorResponse($"Ошибка при удалении места: {ex.Message}");
            }
        }
    }
}
