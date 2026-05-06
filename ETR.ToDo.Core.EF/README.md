# ETR.ToDo.Core.EF — Infrastructure Layer

EF Core infrastructure: DbContext, Fluent API configurations, repository implementation, and migrations.

---

## What Lives Here

```
ETR.ToDo.Core.EF/
├── Configurations/           IEntityTypeConfiguration<T> per entity
├── Context/
│   └── ToDoDbContext.cs      DbContext with audit tracking + soft delete
├── Factories/
│   └── ToDoDbContextFactory.cs  Design-time factory for migrations
├── Migrations/               Auto-generated EF migrations
└── Repositories/
    └── Repository.cs         Generic IRepository<T,K> implementation
```

---

## ToDoDbContext

Overrides `SaveChangesAsync()` to:
1. Auto-populate `CreatedDate`, `CreatedBy` on insert
2. Auto-populate `ModifiedDate`, `ModifiedBy` on update
3. Convert deletes into soft deletes (`IsDeleted = true`, `DeletedDate`, `DeletedBy`)

---

## Fluent API Rules

All schema configuration belongs here — not on entity classes.

| Use Fluent API for | Reason |
|---|---|
| Indexes (`HasIndex`) | No annotation equivalent |
| Unique constraints | No annotation equivalent |
| SQL defaults (`HasDefaultValueSql`) | No annotation equivalent |
| Cascade delete (`OnDelete`) | No annotation equivalent |
| Soft delete filter (`HasQueryFilter`) | No annotation equivalent |
| Relationships (`HasOne`, `WithMany`) | Explicit control |

**Never duplicate** in Fluent API what a `[Required]` or `[MaxLength]` annotation already declares on the entity.

---

## Working with Migrations

```bash
# Add a migration (run from solution root)
dotnet ef migrations add <Name> \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

# Migrations apply automatically on startup via MigrateAsync()
# Manual apply (fallback only):
dotnet ef database update \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

# Rollback
dotnet ef database update <PreviousMigrationName> \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

# Remove last migration
dotnet ef migrations remove \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host

# List migrations
dotnet ef migrations list \
  --project ETR.ToDo.Core.EF \
  --startup-project ETR.ToDo.Web.Host
```

---

## Adding a New Entity Config

1. Create `Configurations/{Entity}Configuration.cs` implementing `IEntityTypeConfiguration<T>`
2. Register in `ToDoDbContext.OnModelCreating()`:
   ```csharp
   modelBuilder.ApplyConfiguration(new MyEntityConfiguration());
   ```
3. Add `DbSet<T>` to `ToDoDbContext`
4. Run `dotnet ef migrations add Add{Entity}`
