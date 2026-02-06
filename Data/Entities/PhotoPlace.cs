using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class PhotoPlace
{
    public int Id { get; set; }

    public int PlaceId { get; set; }

    public int PhotoId { get; set; }

    public bool? IsMain { get; set; }

    public virtual Photo Photo { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;
}
