using System.Text.Json.Serialization;

namespace MetaPlApi.Models.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Errors { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationInfo Pagination { get; set; }
        
        public static ApiResponse<T> SuccessResponse(T data, string message = "Успешно")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        
        public static ApiResponse<T> ErrorResponse(string message, List<string> errors = null)
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
        
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
    
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
    
    public class UserResponse
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public int ApplicationsId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}