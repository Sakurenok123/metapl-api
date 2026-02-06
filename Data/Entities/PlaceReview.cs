using System;

namespace MetaPlApi.Data.Entities;

public partial class PlaceReview
{
    public int Id { get; set; }
    public int PlaceId { get; set; }
    public int UserId { get; set; }
    /// <summary>Оценка от 1 до 5</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Place Place { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
