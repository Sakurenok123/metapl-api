using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class ServicesApplication
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int ServiceId { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
