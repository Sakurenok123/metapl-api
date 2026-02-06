using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Application
{
    public int Id { get; set; }

    public int StatusId { get; set; }

    public int PlaceId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public string? ContactPhone { get; set; }

    public int? GuestCount { get; set; }

    public DateOnly? EventDate { get; set; }

    public TimeOnly? EventTime { get; set; }

    public int? Duration { get; set; }

    public string? AdditionalInfo { get; set; }

    public DateTime? DateCreate { get; set; }

    public DateTime? DateUpdate { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;

    public virtual ICollection<ServicesApplication> ServicesApplications { get; set; } = new List<ServicesApplication>();

    public virtual Status Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
