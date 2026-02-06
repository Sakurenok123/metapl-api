using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Characteristic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CharacteristicsPlace> CharacteristicsPlaces { get; set; } = new List<CharacteristicsPlace>();
}
