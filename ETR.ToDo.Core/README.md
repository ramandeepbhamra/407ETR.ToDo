# ETR.ToDo.Core — Domain Layer

Pure domain layer. Zero external NuGet dependencies.

---

## What Lives Here

```
ETR.ToDo.Core/
├── Entities/
│   ├── Base/
│   │   └── BaseEntity.cs          Generic base (Id, audit fields, soft delete)
│   ├── Users/
│   │   └── User.cs
│   ├── Todos/
│   │   ├── TaskList.cs
│   │   ├── TodoTask.cs
│   │   └── SubTask.cs
│   └── Auth/
│       └── RefreshToken.cs
├── Repositories/
│   └── IRepository.cs             IRepository<TEntity, TKey>
└── Extensions/
    └── QueryableExtensions.cs     WhereIf, PageBy, OrderByIf
```

---

## BaseEntity\<Guid\>

All entities extend `BaseEntity<Guid>`:

| Property | Description |
|---|---|
| `Id` | GUID primary key |
| `CreatedDate` / `CreatedBy` | Auto-set on insert by `ToDoDbContext` |
| `ModifiedDate` / `ModifiedBy` | Auto-set on update by `ToDoDbContext` |
| `IsDeleted` / `DeletedDate` / `DeletedBy` | Soft delete — never hard delete |

**Navigation properties only are `virtual`** (for EF lazy loading). Scalar properties (`string`, `bool`, `DateTime?`) are never `virtual`.

---

## IRepository\<TEntity, TKey\>

Generic repository interface used by all services:

```csharp
IQueryable<TEntity> GetAll();
Task<TEntity?> GetByIdAsync(TKey id);
Task InsertAsync(TEntity entity);
Task UpdateAsync(TEntity entity);
Task DeleteAsync(TEntity entity);   // sets IsDeleted = true
Task SaveChangesAsync();
```

Implemented by `Repository<TEntity, TKey>` in `ETR.ToDo.Core.EF`.

---

## QueryableExtensions

```csharp
query.WhereIf(condition, predicate)         // conditional filter
query.PageBy(skip, take)                    // pagination
query.PageByPageNumber(pageNumber, size)    // page-number pagination
query.OrderByIf(condition, keySelector)     // conditional ordering
```

---

## Rules

- No business logic — belongs in `ETR.ToDo.Services`
- No EF Core references — belongs in `ETR.ToDo.Core.EF`
- No HTTP concerns — belongs in `ETR.ToDo.Web.Host`
- No data annotations for EF config — use Fluent API in `ETR.ToDo.Core.EF`
