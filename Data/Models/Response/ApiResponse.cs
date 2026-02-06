using System.Text.Json.Serialization;

namespace MetaPlApi.Models.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationInfo? Pagination { get; set; }
        
        public static ApiResponse<T> SuccessResponse(T? data, string message = "Успешно")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        
        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
    
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
    
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
    
    public class PlaceResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public AddressInfo? Address { get; set; }
        public List<ServiceInfo>? Services { get; set; } = new List<ServiceInfo>();
        public List<EquipmentInfo>? Equipments { get; set; } = new List<EquipmentInfo>();
        public List<CharacteristicInfo>? Characteristics { get; set; } = new List<CharacteristicInfo>();
        public List<PhotoInfo>? Photos { get; set; } = new List<PhotoInfo>();
        public List<ApplicationInfo>? Applications { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
    
    public class AddressInfo
    {
        public int Id { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public string FullAddress => $"{City}, {Street}, {House}";
    }
    
    public class EquipmentInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    
    public class CharacteristicInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    
    public class ServiceInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    
    public class PhotoInfo
    {
        public int Id { get; set; }
        public string? Url { get; set; }
    }
    
    public class ApplicationInfo
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public DateTime? DateCreate { get; set; }
    }
    
    public class EventResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public int EventTypeId { get; set; }
        public string? EventTypeName { get; set; } = string.Empty;
    }
    
    public class StatusResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
    }
}
