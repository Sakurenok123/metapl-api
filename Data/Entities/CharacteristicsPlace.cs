using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class CharacteristicsPlace
{
    public int Id { get; set; }

    public int PlaceId { get; set; }

    public int CharacteristicId { get; set; }

    public virtual Characteristic Characteristic { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;
}
