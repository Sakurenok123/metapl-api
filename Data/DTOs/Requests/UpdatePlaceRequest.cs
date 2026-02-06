using System.ComponentModel.DataAnnotations;

namespace MetaPlApi.Models.DTOs.Requests;

public class UpdatePlaceRequest
{
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string? Name { get; set; }

    [Range(1, int.MaxValue)]
    public int? AddressesId { get; set; }

    /// <summary>Список ID оборудования (заменяет текущие связи).</summary>
    public List<int>? EquipmentsIds { get; set; }

    /// <summary>Список ID характеристик (заменяет текущие связи).</summary>
    public List<int>? CharacteristicsIds { get; set; }

    /// <summary>Список ID услуг (заменяет текущие связи).</summary>
    public List<int>? ServiceIds { get; set; }

    /// <summary>Список ID фото (заменяет текущие связи).</summary>
    public List<int>? PhotoIds { get; set; }
}