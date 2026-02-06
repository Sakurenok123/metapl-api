using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class EventsType
{
    public int Id { get; set; }

    public string NameEventsType { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
