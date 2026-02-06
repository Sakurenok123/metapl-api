using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class ServicesPlace
{
    public int Id { get; set; }

    public int PlaceId { get; set; }

    public int ServiceId { get; set; }

    public virtual Place Place { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
