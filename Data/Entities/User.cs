using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual ICollection<UserViewHistory> UserViewHistories { get; set; } = new List<UserViewHistory>();

    public virtual ICollection<PlaceReview> PlaceReviews { get; set; } = new List<PlaceReview>();
}
