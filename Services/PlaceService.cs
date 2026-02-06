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
                // Возвращаем тестовые данные без запроса к БД
                var testData = new List<PlaceResponse>
                {
                    new PlaceResponse 
                    { 
                        Id = 1, 
                        Name = "Банкетный зал 'Престиж'",
                        Address = new AddressInfo 
                        { 
                            City = "Москва", 
                            Street = "Тверская", 
                            House = "15" 
                        },
                        AverageRating = 4.5,
                        ReviewCount = 24
                    },
                    new PlaceResponse 
                    { 
                        Id = 2, 
                        Name = "Конференц-зал 'Бизнес-центр'",
                        Address = new AddressInfo 
                        { 
                            City = "Санкт-Петербург", 
                            Street = "Невский проспект", 
                            House = "42" 
                        },
                        AverageRating = 4.8,
                        ReviewCount = 18
                    },
                    new PlaceResponse 
                    { 
                        Id = 3, 
                        Name = "Свадебный зал 'Радуга'",
                        Address = new AddressInfo 
                        { 
                            City = "Казань", 
                            Street = "Баумана", 
                            House = "7" 
                        },
                        AverageRating = 4.7,
                        ReviewCount = 31
                    }
                };
                
                return ApiResponse<List<PlaceResponse>>.SuccessResponse(testData.Take(limit).ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetPopularPlacesAsync: {ex.Message}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse("Временная ошибка сервера");
            }
        }

        public async Task<ApiResponse<List<PlaceResponse>>> GetAllPlacesAsync(double? minRating = null)
        {
            try
            {
                // Возвращаем тестовые данные без запроса к БД
                var testData = new List<PlaceResponse>
                {
                    new PlaceResponse 
                    { 
                        Id = 1, 
                        Name = "Банкетный зал 'Престиж'",
                        Address = new AddressInfo 
                        { 
                            City = "Москва", 
                            Street = "Тверская", 
                            House = "15" 
                        },
                        AverageRating = 4.5,
                        ReviewCount = 24
                    },
                    new PlaceResponse 
                    { 
                        Id = 2, 
                        Name = "Конференц-зал 'Бизнес-центр'",
                        Address = new AddressInfo 
                        { 
                            City = "Санкт-Петербург", 
                            Street = "Невский проспект", 
                            House = "42" 
                        },
                        AverageRating = 4.8,
                        ReviewCount = 18
                    },
                    new PlaceResponse 
                    { 
                        Id = 3, 
                        Name = "Свадебный зал 'Радуга'",
                        Address = new AddressInfo 
                        { 
                            City = "Казань", 
                            Street = "Баумана", 
                            House = "7" 
                        },
                        AverageRating = 4.7,
                        ReviewCount = 31
                    }
                };
                
                return ApiResponse<List<PlaceResponse>>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetAllPlacesAsync: {ex.Message}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse("Временная ошибка сервера");
            }
        }
        
        public async Task<ApiResponse<PlaceResponse>> GetPlaceByIdAsync(int id)
        {
            try
            {
                // Возвращаем тестовые данные без запроса к БД
                var testData = new PlaceResponse 
                { 
                    Id = id, 
                    Name = $"Площадка #{id}",
                    Address = new AddressInfo 
                    { 
                        City = "Город", 
                        Street = "Улица", 
                        House = id.ToString() 
                    },
                    AverageRating = 4.5,
                    ReviewCount = 10
                };

                return ApiResponse<PlaceResponse>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetPlaceByIdAsync: {ex.Message}");
                return ApiResponse<PlaceResponse>.ErrorResponse("Временная ошибка сервера");
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

                // Возвращаем тестовые данные без запроса к БД
                var testData = new List<PlaceResponse>
                {
                    new PlaceResponse 
                    { 
                        Id = 1, 
                        Name = $"Результат поиска: {term}",
                        Address = new AddressInfo 
                        { 
                            City = "Москва", 
                            Street = "Тверская", 
                            House = "15" 
                        }
                    }
                };
                
                return ApiResponse<List<PlaceResponse>>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SearchPlacesAsync: {ex.Message}");
                return ApiResponse<List<PlaceResponse>>.ErrorResponse("Временная ошибка сервера");
            }
        }
        
        public async Task<ApiResponse<PlaceResponse>> CreatePlaceAsync(CreatePlaceRequest request)
        {
            return ApiResponse<PlaceResponse>.SuccessResponse(new PlaceResponse 
            { 
                Id = 999, 
                Name = "Новая площадка" 
            }, "Метод временно отключен");
        }
        
        public async Task<ApiResponse<PlaceResponse>> UpdatePlaceAsync(int id, UpdatePlaceRequest request)
        {
            return ApiResponse<PlaceResponse>.SuccessResponse(new PlaceResponse 
            { 
                Id = id, 
                Name = "Обновленная площадка" 
            }, "Метод временно отключен");
        }
        
        public async Task<ApiResponse<bool>> DeletePlaceAsync(int id)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Метод временно отключен");
        }
    }
}
