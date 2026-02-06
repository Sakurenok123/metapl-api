// PlaceResponses.cs
namespace MetaPlApi.Models.DTOs.Responses
{
public class PlaceResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AddressInfo Address { get; set; } = new AddressInfo();
    
    // ✅ Исправлено: теперь это List<ServiceInfo>, а не List<string>
    public List<ServiceInfo> Services { get; set; } = new List<ServiceInfo>();
    public List<EquipmentInfo> Equipments { get; set; } = new List<EquipmentInfo>();
    public List<CharacteristicInfo> Characteristics { get; set; } = new List<CharacteristicInfo>();
    public List<PhotoInfo> Photos { get; set; } = new List<PhotoInfo>();
    public List<ApplicationInfo>? Applications { get; set; }
    /// <summary>Средняя оценка от 1 до 5 (null если отзывов нет)</summary>
    public double? AverageRating { get; set; }
    /// <summary>Количество отзывов</summary>
    public int ReviewCount { get; set; }

    // ✅ Добавлено: тип мероприятия (если нужен в будущем — например, для фильтрации)
    // public List<EventTypeInfo> EventTypes { get; set; } = new List<EventTypeInfo>();
}

public class AddressInfo
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;

    // Автоматически генерируем полный адрес
    public string FullAddress => $"{City}, {Street}, {House}";
}
    public class EquipmentInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CharacteristicInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ServiceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PhotoInfo
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class ApplicationInfo
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DateCreate { get; set; }
    }
}