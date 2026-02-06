using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ServicesApplication> ServicesApplications { get; set; } = new List<ServicesApplication>();

    public virtual ICollection<ServicesPlace> ServicesPlaces { get; set; } = new List<ServicesPlace>();
}
