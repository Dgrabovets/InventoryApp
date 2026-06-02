using InventoryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventoryApp.Infrastructure.Data
{
    /// <summary>
    /// Контекст базы данных SQLite для системы инвентаризации (локальное приложение)
    /// </summary>
    public class ApplicationContext : DbContext
    {
        #region Конструкторы

        /// <summary>
        /// Создаёт контекст с заданными параметрами
        /// </summary>
        /// <param name="options">Параметры настройки контекста базы данных</param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        #endregion Конструкторы

        #region Свойства

        /// <summary>
        /// Набор данных для ролей пользователей
        /// </summary>
        public DbSet<Role> Roles => Set<Role>();

        /// <summary>
        /// Набор данных для пользователей системы
        /// </summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Набор данных для токенов восстановления пароля
        /// </summary>
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        /// <summary>
        /// Набор данных для строений
        /// </summary>
        public DbSet<Building> Buildings => Set<Building>();

        /// <summary>
        /// Набор данных для этажей строений
        /// </summary>
        public DbSet<Floor> Floors => Set<Floor>();

        /// <summary>
        /// Набор данных для комнат помещений
        /// </summary>
        public DbSet<Room> Rooms => Set<Room>();

        /// <summary>
        /// Набор данных для типов контейнеров
        /// </summary>
        public DbSet<ContainerType> ContainerTypes => Set<ContainerType>();

        /// <summary>
        /// Набор данных для контейнеров хранения
        /// </summary>
        public DbSet<Container> Containers => Set<Container>();

        /// <summary>
        /// Набор данных для отделений контейнеров
        /// </summary>
        public DbSet<Section> Sections => Set<Section>();

        /// <summary>
        /// Набор данных для категорий предметов
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Набор данных для предметов инвентаря
        /// </summary>
        public DbSet<Item> Items => Set<Item>();

        /// <summary>
        /// Набор данных для изображений предметов
        /// </summary>
        public DbSet<ItemImage> ItemImages => Set<ItemImage>();

        /// <summary>
        /// Набор данных для истории перемещений предметов
        /// </summary>
        public DbSet<ItemMovement> ItemMovements => Set<ItemMovement>();

        /// <summary>
        /// Набор данных для аудита действий в системе
        /// </summary>
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        #endregion Свойства

        #region Настройка модели

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация сущности Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(r => r.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
                entity.Property(r => r.Description).HasColumnName("description");
                entity.Property(r => r.CreatedAt).HasColumnName("created_at");
                entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(r => r.Name).IsUnique().HasDatabaseName("IX_roles_name");
            });

            // Конфигурация сущности User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(u => u.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255);
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                entity.Property(u => u.RoleId).HasColumnName("role_id");
                entity.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(u => u.CreatedAt).HasColumnName("created_at");
                entity.Property(u => u.LastLogin).HasColumnName("last_login");
                entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_users_username");
                entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_users_email");

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("password_reset_tokens");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(p => p.Token).HasColumnName("token").HasMaxLength(128).IsRequired();
                entity.Property(p => p.ExpiresAt).HasColumnName("expires_at").IsRequired();
                entity.Property(p => p.IsUsed).HasColumnName("is_used").IsRequired().HasDefaultValue(false);
                entity.Property(p => p.UsedAt).HasColumnName("used_at");
                entity.Property(p => p.CreatedAt).HasColumnName("created_at");
                entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.Token).IsUnique().HasDatabaseName("IX_password_reset_tokens_token");
                entity.HasIndex(p => p.UserId).HasDatabaseName("idx_password_reset_tokens_user_id");
            });

            // Конфигурация сущности Building
            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("buildings");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(b => b.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(b => b.Description).HasColumnName("description");
                entity.Property(b => b.CreatedById).HasColumnName("created_by");
                entity.Property(b => b.CreatedAt).HasColumnName("created_at");
                entity.Property(b => b.UpdatedAt).HasColumnName("updated_at");
                entity.Property(b => b.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(b => b.CreatedBy)
                    .WithMany(u => u.BuildingsCreated)
                    .HasForeignKey(b => b.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности Floor
            modelBuilder.Entity<Floor>(entity =>
            {
                entity.ToTable("floors");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(f => f.BuildingId).HasColumnName("building_id");
                entity.Property(f => f.Level).HasColumnName("level").IsRequired();
                entity.Property(f => f.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(f => f.Description).HasColumnName("description");
                entity.Property(f => f.CreatedById).HasColumnName("created_by");
                entity.Property(f => f.CreatedAt).HasColumnName("created_at");
                entity.Property(f => f.UpdatedAt).HasColumnName("updated_at");
                entity.Property(f => f.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(f => f.Building)
                    .WithMany(b => b.Floors)
                    .HasForeignKey(f => f.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.CreatedBy)
                    .WithMany()
                    .HasForeignKey(f => f.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => f.BuildingId).HasDatabaseName("idx_floors_building_id");
            });

            // Конфигурация сущности Room
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("rooms");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(r => r.FloorId).HasColumnName("floor_id");
                entity.Property(r => r.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(r => r.Description).HasColumnName("description");
                entity.Property(r => r.CreatedById).HasColumnName("created_by");
                entity.Property(r => r.CreatedAt).HasColumnName("created_at");
                entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");
                entity.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(r => r.Floor)
                    .WithMany(f => f.Rooms)
                    .HasForeignKey(r => r.FloorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.CreatedBy)
                    .WithMany()
                    .HasForeignKey(r => r.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.FloorId).HasDatabaseName("idx_rooms_floor_id");
            });

            // Конфигурация сущности ContainerType
            modelBuilder.Entity<ContainerType>(entity =>
            {
                entity.ToTable("container_types");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(t => t.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
                entity.Property(t => t.Description).HasColumnName("description");
                entity.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(t => t.CreatedAt).HasColumnName("created_at");
                entity.Property(t => t.UpdatedAt).HasColumnName("updated_at");
                entity.HasIndex(t => t.IsActive).HasDatabaseName("idx_container_types_is_active");
            });

            // Конфигурация сущности Container
            modelBuilder.Entity<Container>(entity =>
            {
                entity.ToTable("containers");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.FloorId).HasColumnName("floor_id");
                entity.Property(c => c.RoomId).HasColumnName("room_id");
                entity.Property(c => c.ContainerTypeId).HasColumnName("container_type_id");
                entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.LocationDescription).HasColumnName("location_description");
                entity.Property(c => c.CreatedById).HasColumnName("created_by");
                entity.Property(c => c.CreatedAt).HasColumnName("created_at");
                entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
                entity.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(c => c.ContainerType)
                    .WithMany(t => t.Containers)
                    .HasForeignKey(c => c.ContainerTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Floor)
                    .WithMany(f => f.Containers)
                    .HasForeignKey(c => c.FloorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Room)
                    .WithMany(r => r.Containers)
                    .HasForeignKey(c => c.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.CreatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.FloorId).HasDatabaseName("idx_containers_floor_id");
                entity.HasIndex(c => c.RoomId).HasDatabaseName("idx_containers_room_id");
                entity.HasIndex(c => c.ContainerTypeId).HasDatabaseName("idx_containers_container_type_id");
            });

            // Конфигурация сущности Section
            modelBuilder.Entity<Section>(entity =>
            {
                entity.ToTable("sections");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(s => s.ContainerId).HasColumnName("container_id");
                entity.Property(s => s.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(s => s.PositionIndex).HasColumnName("position_index");
                entity.Property(s => s.Description).HasColumnName("description");
                entity.Property(s => s.CreatedById).HasColumnName("created_by");
                entity.Property(s => s.CreatedAt).HasColumnName("created_at");
                entity.Property(s => s.UpdatedAt).HasColumnName("updated_at");
                entity.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(s => s.Container)
                    .WithMany(c => c.Sections)
                    .HasForeignKey(s => s.ContainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.CreatedBy)
                    .WithMany()
                    .HasForeignKey(s => s.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasColumnName("description");
                entity.Property(c => c.ParentCategoryId).HasColumnName("parent_category_id");
                entity.Property(c => c.CreatedById).HasColumnName("created_by");
                entity.Property(c => c.CreatedAt).HasColumnName("created_at");
                entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
                entity.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.ChildCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.CreatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(i => i.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
                entity.Property(i => i.Description).HasColumnName("description");
                entity.Property(i => i.CategoryId).HasColumnName("category_id");
                entity.Property(i => i.Quantity).HasColumnName("quantity").HasDefaultValue(1);
                entity.Property(i => i.ExpiryDate).HasColumnName("expiry_date");
                entity.Property(i => i.ManufactureDate).HasColumnName("manufacture_date");
                entity.Property(i => i.PurchaseDate).HasColumnName("purchase_date");
                entity.Property(i => i.Price).HasColumnName("price").HasPrecision(10, 2);

                entity.Property(i => i.BuildingId).HasColumnName("building_id");
                entity.Property(i => i.FloorId).HasColumnName("floor_id");
                entity.Property(i => i.RoomId).HasColumnName("room_id");
                entity.Property(i => i.ContainerId).HasColumnName("container_id");
                entity.Property(i => i.SectionId).HasColumnName("section_id");

                entity.Property(i => i.LocationNotes).HasColumnName("location_notes");
                entity.Property(i => i.CreatedById).HasColumnName("created_by");
                entity.Property(i => i.CreatedAt).HasColumnName("created_at");
                entity.Property(i => i.UpdatedAt).HasColumnName("updated_at");
                entity.Property(i => i.LastCheckedDate).HasColumnName("last_checked_date");
                entity.Property(i => i.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(i => i.Building)
                    .WithMany(b => b.Items)
                    .HasForeignKey(i => i.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Floor)
                    .WithMany(f => f.Items)
                    .HasForeignKey(i => i.FloorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Room)
                    .WithMany(r => r.Items)
                    .HasForeignKey(i => i.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Container)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => i.ContainerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Section)
                    .WithMany(s => s.Items)
                    .HasForeignKey(i => i.SectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Category)
                    .WithMany(c => c.Items)
                    .HasForeignKey(i => i.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.CreatedBy)
                    .WithMany(u => u.ItemsCreated)
                    .HasForeignKey(i => i.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => i.BuildingId).HasDatabaseName("idx_items_building_id");
                entity.HasIndex(i => i.FloorId).HasDatabaseName("idx_items_floor_id");
                entity.HasIndex(i => i.RoomId).HasDatabaseName("idx_items_room_id");
                entity.HasIndex(i => i.ContainerId).HasDatabaseName("idx_items_container_id");
                entity.HasIndex(i => i.SectionId).HasDatabaseName("idx_items_section_id");
                entity.HasIndex(i => i.ExpiryDate).HasDatabaseName("idx_items_expiry_date");
                entity.HasIndex(i => i.CategoryId).HasDatabaseName("idx_items_category_id");
                entity.HasIndex(i => i.IsActive).HasDatabaseName("idx_items_is_active");
                entity.HasIndex(i => i.CreatedAt).HasDatabaseName("idx_items_created_at");
            });

            // Конфигурация сущности ItemImage
            modelBuilder.Entity<ItemImage>(entity =>
            {
                entity.ToTable("item_images");
                entity.HasKey(ii => ii.Id);
                entity.Property(ii => ii.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(ii => ii.ItemId).HasColumnName("item_id");
                entity.Property(ii => ii.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
                entity.Property(ii => ii.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);
                entity.Property(ii => ii.UploadedById).HasColumnName("uploaded_by");
                entity.Property(ii => ii.CreatedAt).HasColumnName("created_at");
                entity.Property(ii => ii.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(ii => ii.Item)
                    .WithMany(i => i.ItemImages)
                    .HasForeignKey(ii => ii.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ii => ii.UploadedBy)
                    .WithMany()
                    .HasForeignKey(ii => ii.UploadedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности ItemMovement
            modelBuilder.Entity<ItemMovement>(entity =>
            {
                entity.ToTable("item_movements");
                entity.HasKey(im => im.Id);
                entity.Property(im => im.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(im => im.ItemName).HasColumnName("item_name").HasMaxLength(200).IsRequired();

                entity.Property(im => im.FromBuildingId).HasColumnName("from_building_id");
                entity.Property(im => im.FromFloorId).HasColumnName("from_floor_id");
                entity.Property(im => im.FromRoomId).HasColumnName("from_room_id");
                entity.Property(im => im.FromContainerId).HasColumnName("from_container_id");
                entity.Property(im => im.FromSectionId).HasColumnName("from_section_id");

                entity.Property(im => im.ToBuildingId).HasColumnName("to_building_id");
                entity.Property(im => im.ToFloorId).HasColumnName("to_floor_id");
                entity.Property(im => im.ToRoomId).HasColumnName("to_room_id");
                entity.Property(im => im.ToContainerId).HasColumnName("to_container_id");
                entity.Property(im => im.ToSectionId).HasColumnName("to_section_id");

                entity.Property(im => im.MovedById).HasColumnName("moved_by");
                entity.Property(im => im.MovedAt).HasColumnName("moved_at");
                entity.Property(im => im.Notes).HasColumnName("notes");
                entity.Property(im => im.CreatedAt).HasColumnName("created_at");
                entity.Property(im => im.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(im => im.MovedBy)
                    .WithMany()
                    .HasForeignKey(im => im.MovedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.FromBuilding)
                    .WithMany()
                    .HasForeignKey(im => im.FromBuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.FromFloor)
                    .WithMany()
                    .HasForeignKey(im => im.FromFloorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.FromRoom)
                    .WithMany()
                    .HasForeignKey(im => im.FromRoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.FromContainer)
                    .WithMany()
                    .HasForeignKey(im => im.FromContainerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.FromSection)
                    .WithMany()
                    .HasForeignKey(im => im.FromSectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.ToBuilding)
                    .WithMany()
                    .HasForeignKey(im => im.ToBuildingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.ToFloor)
                    .WithMany()
                    .HasForeignKey(im => im.ToFloorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.ToRoom)
                    .WithMany()
                    .HasForeignKey(im => im.ToRoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.ToContainer)
                    .WithMany()
                    .HasForeignKey(im => im.ToContainerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(im => im.ToSection)
                    .WithMany()
                    .HasForeignKey(im => im.ToSectionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация сущности AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(al => al.Id);
                entity.Property(al => al.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(al => al.TableName).HasColumnName("table_name").HasMaxLength(100).IsRequired();
                entity.Property(al => al.RecordId).HasColumnName("record_id").IsRequired();
                entity.Property(al => al.ActionType).HasColumnName("action_type").HasMaxLength(10).IsRequired();
                entity.Property(al => al.OldValues).HasColumnName("old_values");
                entity.Property(al => al.NewValues).HasColumnName("new_values");
                entity.Property(al => al.UserId).HasColumnName("user_id");
                entity.Property(al => al.CreatedAt).HasColumnName("created_at");
                entity.Property(al => al.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(al => al.User)
                    .WithMany()
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(al => new { al.TableName, al.RecordId }).HasDatabaseName("idx_audit_logs_table_record");
                entity.HasIndex(al => al.CreatedAt).HasDatabaseName("idx_audit_logs_created_at");
                entity.HasIndex(al => al.UserId).HasDatabaseName("idx_audit_logs_user_id");
            });

            modelBuilder.Entity<Building>()
                .HasIndex(b => b.IsActive)
                .HasDatabaseName("idx_buildings_is_active");

            modelBuilder.Entity<Floor>()
                .HasIndex(f => f.IsActive)
                .HasDatabaseName("idx_floors_is_active");

            modelBuilder.Entity<Room>()
                .HasIndex(r => r.IsActive)
                .HasDatabaseName("idx_rooms_is_active");

            modelBuilder.Entity<Container>()
                .HasIndex(c => c.IsActive)
                .HasDatabaseName("idx_containers_is_active");

            modelBuilder.Entity<Section>()
                .HasIndex(s => s.IsActive)
                .HasDatabaseName("idx_sections_is_active");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.IsActive)
                .HasDatabaseName("idx_categories_is_active");
        }

        #endregion Настройка модели

        #region Публичные методы

        /// <inheritdoc />
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <inheritdoc />
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <inheritdoc />
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion Публичные методы

        #region Приватные методы

        /// <summary>
        /// Обновляет временные метки для сущностей перед сохранением
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        #endregion Приватные методы
    }
}
