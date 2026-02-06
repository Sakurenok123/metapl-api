using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MetaPlApi.Models.DTOs.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string Login { get; set; }
        
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        public string Password { get; set; }
    }
    
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string Login { get; set; }
        
        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        public string Password { get; set; }
        
        // Сделать RoleId необязательным - будет установлен по умолчанию
        public int RoleId { get; set; } = 3; // Значение по умолчанию = Пользователь
        
        // Сделать ApplicationsId необязательным
        public int? ApplicationsId { get; set; }
    }
    
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Старый пароль обязателен")]
        public string OldPassword { get; set; }
        
        [Required(ErrorMessage = "Новый пароль обязателен")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 20 символов")]
        public string NewPassword { get; set; }
    }
    
    public class UpdateProfileRequest
    {
        [JsonPropertyName("login")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string Login { get; set; }
        
        [JsonPropertyName("roleId")]
        [Range(1, int.MaxValue, ErrorMessage = "Выберите роль от 1 и выше")]
        public int? RoleId { get; set; }
    }

    /// <summary>Тело запроса только для смены роли пользователя (принимает roleId в любом регистре ключа).</summary>
    public class ChangeUserRoleRequest
    {
        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }
    }
}