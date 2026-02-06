using System.ComponentModel.DataAnnotations;

namespace MetaPlApi.Models.DTOs.Requests
{
    public class CreatePlaceRequest
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID адреса обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Укажите ID адреса")]
        public int AddressesId { get; set; }

        public List<int>? EquipmentsIds { get; set; }
        public List<int>? CharacteristicsIds { get; set; }
        public List<int>? ServiceIds { get; set; }
        public List<int>? PhotoIds { get; set; }
    }
    
    public class CreateAddressRequest
    {
        [Required(ErrorMessage = "Город обязателен")]
        [StringLength(100, ErrorMessage = "Название города не должно превышать 100 символов")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Улица обязательна")]
        [StringLength(200, ErrorMessage = "Название улицы не должно превышать 200 символов")]
        public string Street { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Номер дома не должен превышать 20 символов")]
        public string House { get; set; } = string.Empty;
    }
    
    public class UpdateAddressRequest
    {
        [StringLength(100, ErrorMessage = "Название города не должно превышать 100 символов")]
        public string City { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Название улицы не должно превышать 200 символов")]
        public string Street { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "Номер дома не должен превышать 20 символов")]
        public string House { get; set; } = string.Empty;
    }
}