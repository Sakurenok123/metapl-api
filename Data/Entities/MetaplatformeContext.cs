using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Data.Entities
{
    public partial class MetaplatformeContext : DbContext
    {
        public MetaplatformeContext()
        {
        }

        public MetaplatformeContext(DbContextOptions<MetaplatformeContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Characteristic> Characteristics { get; set; }
        public virtual DbSet<CharacteristicsPlace> CharacteristicsPlaces { get; set; }
        public virtual DbSet<Equipment> Equipments { get; set; }
        public virtual DbSet<EquipmentsPlace> EquipmentsPlaces { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<EventsType> EventsTypes { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<PhotoPlace> PhotoPlaces { get; set; }
        public virtual DbSet<Place> Places { get; set; }
        public virtual DbSet<PlaceReview> PlaceReviews { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServicesApplication> ServicesApplications { get; set; }
        public virtual DbSet<ServicesPlace> ServicesPlaces { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserFavorite> UserFavorites { get; set; }
        public virtual DbSet<UserViewHistory> UserViewHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Убираем жестко закодированную строку подключения
            // Конфигурация теперь только через DI
            if (!optionsBuilder.IsConfigured)
            {
                // Для разработки можно использовать appsettings.json
                // В Railway конфигурация приходит через переменные окружения
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("addresses_pkey");
                entity.ToTable("addresses");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.City).HasMaxLength(100).HasColumnName("city");
                entity.Property(e => e.House).HasMaxLength(20).HasColumnName("house");
                entity.Property(e => e.Street).HasMaxLength(200).HasColumnName("street");
            });

            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("applications_pkey");
                entity.ToTable("applications");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AdditionalInfo).HasColumnName("additional_info");
                entity.Property(e => e.ContactPhone).HasMaxLength(20).HasColumnName("contact_phone");
                entity.Property(e => e.DateCreate).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("date_create");
                entity.Property(e => e.DateUpdate).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("date_update");
                entity.Property(e => e.Duration).HasColumnName("duration");
                entity.Property(e => e.EventDate).HasColumnName("event_date");
                entity.Property(e => e.EventId).HasColumnName("event_id");
                entity.Property(e => e.EventTime).HasColumnName("event_time");
                entity.Property(e => e.GuestCount).HasColumnName("guest_count");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
                entity.Property(e => e.StatusId).HasColumnName("status_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Event).WithMany(p => p.Applications)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("applications_event_id_fkey");

                entity.HasOne(d => d.Place).WithMany(p => p.Applications)
                    .HasForeignKey(d => d.PlaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("applications_place_id_fkey");

                entity.HasOne(d => d.Status).WithMany(p => p.Applications)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("applications_status_id_fkey");

                entity.HasOne(d => d.User).WithMany(p => p.Applications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("applications_user_id_fkey");
            });

            modelBuilder.Entity<Characteristic>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("characteristics_pkey");
                entity.ToTable("characteristics");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");
            });

            modelBuilder.Entity<CharacteristicsPlace>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("characteristics_places_pkey");
                entity.ToTable("characteristics_places");
                entity.HasIndex(e => new { e.PlaceId, e.CharacteristicId }, "characteristics_places_place_id_characteristic_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CharacteristicId).HasColumnName("characteristic_id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");

                entity.HasOne(d => d.Characteristic).WithMany(p => p.CharacteristicsPlaces)
                    .HasForeignKey(d => d.CharacteristicId)
                    .HasConstraintName("characteristics_places_characteristic_id_fkey");

                entity.HasOne(d => d.Place).WithMany(p => p.CharacteristicsPlaces)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("characteristics_places_place_id_fkey");
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("equipments_pkey");
                entity.ToTable("equipments");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            });

            modelBuilder.Entity<EquipmentsPlace>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("equipments_places_pkey");
                entity.ToTable("equipments_places");
                entity.HasIndex(e => new { e.PlaceId, e.EquipmentId }, "equipments_places_place_id_equipment_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");

                entity.HasOne(d => d.Equipment).WithMany(p => p.EquipmentsPlaces)
                    .HasForeignKey(d => d.EquipmentId)
                    .HasConstraintName("equipments_places_equipment_id_fkey");

                entity.HasOne(d => d.Place).WithMany(p => p.EquipmentsPlaces)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("equipments_places_place_id_fkey");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("events_pkey");
                entity.ToTable("events");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EventTypeId).HasColumnName("event_type_id");
                entity.Property(e => e.Name).HasMaxLength(200).HasColumnName("name");

                entity.HasOne(d => d.EventType).WithMany(p => p.Events)
                    .HasForeignKey(d => d.EventTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("events_event_type_id_fkey");
            });

            modelBuilder.Entity<EventsType>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("events_type_pkey");
                entity.ToTable("events_type");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NameEventsType).HasMaxLength(100).HasColumnName("name_events_type");
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("photo_pkey");
                entity.ToTable("photo");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<PhotoPlace>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("photo_places_pkey");
                entity.ToTable("photo_places");
                entity.HasIndex(e => new { e.PlaceId, e.PhotoId }, "photo_places_place_id_photo_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.IsMain).HasDefaultValue(false).HasColumnName("is_main");
                entity.Property(e => e.PhotoId).HasColumnName("photo_id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");

                entity.HasOne(d => d.Photo).WithMany(p => p.PhotoPlaces)
                    .HasForeignKey(d => d.PhotoId)
                    .HasConstraintName("photo_places_photo_id_fkey");

                entity.HasOne(d => d.Place).WithMany(p => p.PhotoPlaces)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("photo_places_place_id_fkey");
            });

            modelBuilder.Entity<Place>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("places_pkey");
                entity.ToTable("places");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AddressId).HasColumnName("address_id");
                entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");

                entity.HasOne(d => d.Address).WithMany(p => p.Places)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("places_address_id_fkey");
            });

            modelBuilder.Entity<PlaceReview>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("place_reviews_pkey");
                entity.ToTable("place_reviews");
                entity.HasIndex(e => new { e.PlaceId, e.UserId }, "place_reviews_place_id_user_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Comment).HasColumnName("comment");
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").HasColumnName("created_at");
                
                entity.HasOne(d => d.Place).WithMany(p => p.PlaceReviews)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("place_reviews_place_id_fkey");
                    
                entity.HasOne(d => d.User).WithMany(p => p.PlaceReviews)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("place_reviews_user_id_fkey");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("roles_pkey");
                entity.ToTable("roles");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("services_pkey");
                entity.ToTable("services");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            });

            modelBuilder.Entity<ServicesApplication>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("services_application_pkey");
                entity.ToTable("services_application");
                entity.HasIndex(e => new { e.ApplicationId, e.ServiceId }, "services_application_application_id_service_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ApplicationId).HasColumnName("application_id");
                entity.Property(e => e.ServiceId).HasColumnName("service_id");

                entity.HasOne(d => d.Application).WithMany(p => p.ServicesApplications)
                    .HasForeignKey(d => d.ApplicationId)
                    .HasConstraintName("services_application_application_id_fkey");

                entity.HasOne(d => d.Service).WithMany(p => p.ServicesApplications)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("services_application_service_id_fkey");
            });

            modelBuilder.Entity<ServicesPlace>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("services_places_pkey");
                entity.ToTable("services_places");
                entity.HasIndex(e => new { e.PlaceId, e.ServiceId }, "services_places_place_id_service_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
                entity.Property(e => e.ServiceId).HasColumnName("service_id");

                entity.HasOne(d => d.Place).WithMany(p => p.ServicesPlaces)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("services_places_place_id_fkey");

                entity.HasOne(d => d.Service).WithMany(p => p.ServicesPlaces)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("services_places_service_id_fkey");
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("statuses_pkey");
                entity.ToTable("statuses");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("users_pkey");
                entity.ToTable("users");
                entity.HasIndex(e => e.Login, "users_login_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("created_at");
                entity.Property(e => e.Login).HasMaxLength(50).HasColumnName("login");
                entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("users_role_id_fkey");
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("user_favorites_pkey");
                entity.ToTable("user_favorites");
                entity.HasIndex(e => e.PlaceId, "ix_user_favorites_place_id");
                entity.HasIndex(e => e.UserId, "ix_user_favorites_user_id");
                entity.HasIndex(e => new { e.UserId, e.PlaceId }, "user_favorites_user_id_place_id_key").IsUnique();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AddedDate).HasColumnType("timestamp without time zone").HasColumnName("added_date");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Place).WithMany(p => p.UserFavorites)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("user_favorites_place_id_fkey");

                entity.HasOne(d => d.User).WithMany(p => p.UserFavorites)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_favorites_user_id_fkey");
            });

            modelBuilder.Entity<UserViewHistory>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("user_view_history_pkey");
                entity.ToTable("user_view_history");
                entity.HasIndex(e => e.UserId, "ix_user_view_history_user_id");
                entity.HasIndex(e => e.ViewedAt, "ix_user_view_history_viewed_at").IsDescending();
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PlaceId).HasColumnName("place_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ViewedAt).HasColumnType("timestamp without time zone").HasColumnName("viewed_at");

                entity.HasOne(d => d.Place).WithMany(p => p.UserViewHistories)
                    .HasForeignKey(d => d.PlaceId)
                    .HasConstraintName("user_view_history_place_id_fkey");

                entity.HasOne(d => d.User).WithMany(p => p.UserViewHistories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_view_history_user_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
