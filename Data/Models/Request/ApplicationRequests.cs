using System.ComponentModel.DataAnnotations;

namespace MetaPlApi.Models.DTOs.Requests
{
    public class CreateApplicationRequest
    {
        [Required(ErrorMessage = "Название мероприятия обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string EventName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Тип мероприятия обязателен")]
        public int EventTypeId { get; set; }
        
        [Required(ErrorMessage = "Площадка обязательна")]
        public int PlaceId { get; set; }
        
        [Required(ErrorMessage = "Необходимо указать хотя бы одну услугу")]
        public List<int> ServiceIds { get; set; } = new List<int>();
        
        [Required(ErrorMessage = "Контактный телефон обязателен")]
        public string ContactPhone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Количество гостей обязательно")]
        [Range(1, 1000, ErrorMessage = "Количество гостей должно быть от 1 до 1000")]
        public int GuestCount { get; set; }
        
        [Required(ErrorMessage = "Дата мероприятия обязательна")]
        public DateOnly EventDate { get; set; }
        
        [Required(ErrorMessage = "Время мероприятия обязательно")]
        public TimeOnly EventTime { get; set; }
        
        [Required(ErrorMessage = "Продолжительность обязательна")]
        [Range(1, 24, ErrorMessage = "Продолжительность должна быть от 1 до 24 часов")]
        public int Duration { get; set; }
        
        public string? AdditionalInfo { get; set; }
    }
    
    public class UpdateApplicationRequest
    {
        public int? StatusId { get; set; }
        public string? EventName { get; set; }
        public int? EventTypeId { get; set; }
        public int? PlaceId { get; set; }
        public List<int>? ServiceIds { get; set; }
        public string? ContactPhone { get; set; }
        
        [Range(1, 1000, ErrorMessage = "Количество гостей должно быть от 1 до 1000")]
        public int? GuestCount { get; set; }
        public DateOnly? EventDate { get; set; }
        public TimeOnly? EventTime { get; set; }
        
        [Range(1, 24, ErrorMessage = "Продолжительность должна быть от 1 до 24 часов")]
        public int? Duration { get; set; }
        public string? AdditionalInfo { get; set; }
    }
    
     public class ApplicationFilterRequest
    {
        public int? StatusId { get; set; }
        public int? EventId { get; set; }
        public int? PlaceId { get; set; }
        public int? ServiceId { get; set; }
        public string? SearchQuery { get; set; } // Заменяем UserLogin на SearchQuery
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}