using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class EquipmentsPlace
{
    public int Id { get; set; }

    public int PlaceId { get; set; }

    public int EquipmentId { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;
}
