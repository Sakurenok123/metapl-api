using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Photo
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PhotoPlace> PhotoPlaces { get; set; } = new List<PhotoPlace>();
}
