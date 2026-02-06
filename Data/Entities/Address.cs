using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Address
{
    public int Id { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string? House { get; set; }

    public virtual ICollection<Place> Places { get; set; } = new List<Place>();
}
