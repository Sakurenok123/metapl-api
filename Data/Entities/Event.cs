using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Event
{
    public int Id { get; set; }

    public int EventTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual EventsType EventType { get; set; } = null!;
}
