using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class UserFavorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PlaceId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual Place Place { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
