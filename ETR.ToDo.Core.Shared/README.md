# ETR.ToDo.Core.Shared — Shared Library

Shared enums, constants, and config POCOs. Referenced by all layers. Zero dependencies.

---

## What Lives Here

```
ETR.ToDo.Core.Shared/
├── Enums/
│   └── UserRole.cs        Basic, Admin, Dev
├── Constants/
│   └── AppConstants.cs    Limit keys, claim names
└── Config/
    └── AppConfig.cs       Bound from appsettings.json
```

---

## Rules

- No business logic — belongs in `ETR.ToDo.Services`
- No entity classes — belongs in `ETR.ToDo.Core`
- No EF or HTTP references
- Only add items used by **two or more** projects

---

## UserRole

```csharp
public enum UserRole
{
    Basic = 1,
    Admin = 2,
    Dev = 3,
}
```

Used in JWT claims, authorization guards, and role-based UI rendering.
