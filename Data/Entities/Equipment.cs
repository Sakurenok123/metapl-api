using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Equipment
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<EquipmentsPlace> EquipmentsPlaces { get; set; } = new List<EquipmentsPlace>();
}
