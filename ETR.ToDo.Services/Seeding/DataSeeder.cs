using BCrypt.Net;
using ETR.ToDo.Core.Entities.Users;using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Core.Shared.Enums;
using ETR.ToDo.Services.Core.Seeding;
using Microsoft.EntityFrameworkCore;

namespace ETR.ToDo.Services.Seeding;

/// <summary>
/// Seeds initial users for every role on application startup.
/// Idempotent — skips any user whose email already exists in the database.
/// </summary>
public class DataSeeder : IDataSeeder
{
    private const string DefaultPassword = "123qwe";

    private static readonly (string First, string Last, string Email, UserRole Role, bool IsSystemUser)[] Seeds =
    [
        // BASIC (10)
        (First: "Arnold",    Last: "Schwarzenegger", Email: "ETR.basic1@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: true),
        (First: "Jim",       Last: "Carrey",         Email: "ETR.basic2@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: true),
        (First: "Will",      Last: "Smith",          Email: "ETR.basic3@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Leonardo",  Last: "DiCaprio",       Email: "ETR.basic4@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Morgan",    Last: "Freeman",        Email: "ETR.basic5@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Denzel",    Last: "Washington",     Email: "ETR.basic6@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Chris",     Last: "Pratt",          Email: "ETR.basic7@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Matt",      Last: "Damon",          Email: "ETR.basic8@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Hugh",      Last: "Jackman",        Email: "ETR.basic9@yopmail.com",   Role: UserRole.Basic,   IsSystemUser: false),
        (First: "Ryan",      Last: "Reynolds",       Email: "ETR.basic10@yopmail.com",  Role: UserRole.Basic,   IsSystemUser: false),

        // PREMIUM (10)
        (First: "Brad",      Last: "Pitt",           Email: "ETR.premium1@yopmail.com", Role: UserRole.Premium, IsSystemUser: true),
        (First: "Angelina",  Last: "Jolie",          Email: "ETR.premium2@yopmail.com", Role: UserRole.Premium, IsSystemUser: true),
        (First: "Johnny",    Last: "Depp",           Email: "ETR.premium3@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Tom",       Last: "Cruise",         Email: "ETR.premium4@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Robert",    Last: "DeNiro",         Email: "ETR.premium5@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Al",        Last: "Pacino",         Email: "ETR.premium6@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Christian", Last: "Bale",           Email: "ETR.premium7@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Henry",     Last: "Cavill",         Email: "ETR.premium8@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Chris",     Last: "Hemsworth",      Email: "ETR.premium9@yopmail.com", Role: UserRole.Premium, IsSystemUser: false),
        (First: "Mark",      Last: "Wahlberg",       Email: "ETR.premium10@yopmail.com",Role: UserRole.Premium, IsSystemUser: false),

        // ADMIN (10)
        (First: "Robert",    Last: "Downey",         Email: "ETR.admin1@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: true),
        (First: "Scarlett",  Last: "Johansson",      Email: "ETR.admin2@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: true),
        (First: "Natalie",   Last: "Portman",        Email: "ETR.admin3@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Emma",      Last: "Stone",          Email: "ETR.admin4@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Jennifer",  Last: "Lawrence",       Email: "ETR.admin5@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Anne",      Last: "Hathaway",       Email: "ETR.admin6@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Gal",       Last: "Gadot",          Email: "ETR.admin7@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Margot",    Last: "Robbie",         Email: "ETR.admin8@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Zendaya",   Last: "Coleman",        Email: "ETR.admin9@yopmail.com",   Role: UserRole.Admin,   IsSystemUser: false),
        (First: "Charlize",  Last: "Theron",         Email: "ETR.admin10@yopmail.com",  Role: UserRole.Admin,   IsSystemUser: false),

        // DEV (10)
        (First: "Keanu",     Last: "Reeves",         Email: "ETR.dev1@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: true),
        (First: "Tom",       Last: "Hanks",          Email: "ETR.dev2@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: true),
        (First: "Benedict",  Last: "Cumberbatch",    Email: "ETR.dev3@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Andrew",    Last: "Garfield",       Email: "ETR.dev4@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Tobey",     Last: "Maguire",        Email: "ETR.dev5@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Daniel",    Last: "Radcliffe",      Email: "ETR.dev6@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Elijah",    Last: "Wood",           Email: "ETR.dev7@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Orlando",   Last: "Bloom",          Email: "ETR.dev8@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Chris",     Last: "Evans",          Email: "ETR.dev9@yopmail.com",     Role: UserRole.Dev,     IsSystemUser: false),
        (First: "Sebastian", Last: "Stan",           Email: "ETR.dev10@yopmail.com",    Role: UserRole.Dev,     IsSystemUser: false),
    ];

    private readonly IRepository<User, Guid> _userRepository;

    public DataSeeder(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword, workFactor: 12);

        foreach (var seed in Seeds)
        {
            var exists = await _userRepository
                .GetAll()
                .AnyAsync(u => u.Email == seed.Email, cancellationToken);

            if (exists)
                continue;

            await _userRepository.InsertAsync(new User
            {
                Id           = Guid.NewGuid(),
                FirstName    = seed.First,
                LastName     = seed.Last,
                Email        = seed.Email,
                PasswordHash = passwordHash,
                Role         = seed.Role,
                IsActive     = true,
                IsSystemUser = seed.IsSystemUser,
            }, cancellationToken);
        }
    }
}
