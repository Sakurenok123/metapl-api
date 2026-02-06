using System;
using System.Collections.Generic;

namespace MetaPlApi.Data.Entities;

public partial class Place
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int AddressId { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CharacteristicsPlace> CharacteristicsPlaces { get; set; } = new List<CharacteristicsPlace>();

    public virtual ICollection<EquipmentsPlace> EquipmentsPlaces { get; set; } = new List<EquipmentsPlace>();

    public virtual ICollection<PhotoPlace> PhotoPlaces { get; set; } = new List<PhotoPlace>();

    public virtual ICollection<ServicesPlace> ServicesPlaces { get; set; } = new List<ServicesPlace>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual ICollection<UserViewHistory> UserViewHistories { get; set; } = new List<UserViewHistory>();

    public virtual ICollection<PlaceReview> PlaceReviews { get; set; } = new List<PlaceReview>();
}
