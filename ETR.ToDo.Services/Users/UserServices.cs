using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Core.Extensions;
using ETR.ToDo.Core.Repositories;
using ETR.ToDo.Services.Core.Authentication;
using ETR.ToDo.Services.Core.Common.Dto;
using ETR.ToDo.Services.Core.Users;
using ETR.ToDo.Services.Core.Users.Dto;
using ETR.ToDo.Services.Mapping;
using Microsoft.EntityFrameworkCore;

namespace ETR.ToDo.Services.Users;

/// <summary>
/// User service implementation with complete CRUD operations
/// Uses generic repository only - all custom queries built in service layer
/// All business logic (filtering, sorting, searching) is handled in the service layer
/// </summary>
public class UserServices : IUserService
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly UserMapper _mapper;

    public UserServices(
        IRepository<User, Guid> userRepository,
        IAuthenticationService authenticationService,
        UserMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResultDto<UserDto>> GetUsersAsync(GetUsersInputDto input, CancellationToken cancellationToken = default)
    {
        // Build filtered query
        var query = GetUsersFilteredQuery(input);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, input.SortBy, input.SortDirection);

        // Apply pagination
        var skip = (input.Page - 1) * input.PageSize;
        query = query.Skip(skip).Take(input.PageSize);

        // Execute query
        var users = await query.ToListAsync(cancellationToken);

        // Map to DTOs
        var userDtos = users.Select(u => _mapper.UserToUserDto(u)).ToList();

        return new PagedResultDto<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = input.Page,
            PageSize = input.PageSize
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }

        return _mapper.UserToUserDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto input, CancellationToken cancellationToken = default)
    {
        // Check if email already exists using generic repository
        var existingUser = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(u => u.Email == input.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"Email '{input.Email}' is already in use.");
        }

        // Hash password
        var passwordHash = _authenticationService.HashPassword(input.Password);

        // Map DTO to entity
        var user = _mapper.CreateUserDtoToUser(input);
        user.PasswordHash = passwordHash;

        // Save to database
        var createdUser = await _userRepository.InsertAsync(user, cancellationToken);

        return _mapper.UserToUserDto(createdUser);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }

        // Guard: system user role cannot be changed
        if (user.IsSystemUser && user.Role != input.Role)
            throw new InvalidOperationException("System user role cannot be changed.");

        // Check if email is being changed and if new email already exists
        if (user.Email != input.Email)
        {
            var existingUser = await _userRepository
                .GetAll()
                .FirstOrDefaultAsync(u => u.Email == input.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"Email '{input.Email}' is already in use.");
            }
        }

        // Map changes to entity
        _mapper.UpdateUserDtoToUser(input, user);

        // Save changes
        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        return _mapper.UserToUserDto(updatedUser);
    }

    /// <summary>
    /// Updates the current user's own profile (FirstName, LastName, PhoneNumber only).
    /// This method is separate from <see cref="UpdateUserAsync"/> to enforce restricted field access.
    /// Users cannot change their email, role, or password through this endpoint.
    /// </summary>
    /// <param name="userId">The ID of the user whose profile is being updated.</param>
    /// <param name="input">The profile update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user DTO.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileDto input, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID '{userId}' not found.");

        user.FirstName   = input.FirstName.Trim();
        user.LastName    = input.LastName.Trim();
        user.PhoneNumber = input.PhoneNumber?.Trim();

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.UserToUserDto(updated);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }

        // Soft delete
        await _userRepository.DeleteAsync(user, cancellationToken);
    }

    /// <summary>
    /// Get simple list of users with filtering (existing method - kept for backward compatibility)
    /// </summary>
    public async Task<List<UserListDto>> GetUser(GetUserInputDto input)
    {
        var query = GetUserFilteredQuery(input);

        query = query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName);

        var users = await query.ToListAsync();

        return _mapper.UsersToUserListDtos(users);
    }

    #region Private Query Builders

    /// <summary>
    /// Builds filtered query for paginated user list
    /// </summary>
    private IQueryable<User> GetUsersFilteredQuery(GetUsersInputDto input)
    {
        var query = _userRepository.GetAll();

        // Apply IsActive filter
        if (input.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == input.IsActive.Value);
        }

        // Apply IsSystemUser filter
        if (input.IsSystemUser.HasValue)
        {
            query = query.Where(u => u.IsSystemUser == input.IsSystemUser.Value);
        }

        // Exclude the requesting user from the list
        if (input.ExcludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != input.ExcludeUserId.Value);
        }

        // Apply Email filter
        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Email),
            u => u.Email.ToLower().Contains(input.Email!.Trim().ToLower()));

        // Apply Name filter (FirstName or LastName)
        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.Name),
            u => u.FirstName.ToLower().Contains(input.Name!.Trim().ToLower()) ||
                 u.LastName.ToLower().Contains(input.Name!.Trim().ToLower()));

        // Apply Role filter
        if (input.Role.HasValue)
            query = query.Where(u => u.Role == input.Role.Value);

        // Apply SearchKeyword (searches across Email, FirstName, LastName)
        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(input.SearchKeyword),
            u => u.Email.ToLower().Contains(input.SearchKeyword!.Trim().ToLower()) ||
                 u.FirstName.ToLower().Contains(input.SearchKeyword!.Trim().ToLower()) ||
                 u.LastName.ToLower().Contains(input.SearchKeyword!.Trim().ToLower()));

        return query;
    }

    /// <summary>
    /// Builds filtered query for simple user list (backward compatibility)
    /// </summary>
    private IQueryable<User> GetUserFilteredQuery(GetUserInputDto? input)
    {
        var query = _userRepository.GetAll();

        // Active users only
        query = query.Where(u => u.IsActive);

        // Search filter
        query = query.WhereIf(
            input?.Filter.HasValue() ?? false,
            u => u.Email.ToLower().Contains(input!.Filter!.Trim().ToLower()) ||
                 u.FirstName.ToLower().Contains(input.Filter.Trim().ToLower()) ||
                 u.LastName.ToLower().Contains(input.Filter.Trim().ToLower()));

        return query;
    }

    /// <summary>
    /// Applies sorting to query
    /// </summary>
    private IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, string sortDirection)
    {
        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "email"       => isDescending ? query.OrderByDescending(u => u.Email)       : query.OrderBy(u => u.Email),
            "firstname"   => isDescending ? query.OrderByDescending(u => u.FirstName)   : query.OrderBy(u => u.FirstName),
            "lastname"    => isDescending ? query.OrderByDescending(u => u.LastName)    : query.OrderBy(u => u.LastName),
            "role"        => isDescending ? query.OrderByDescending(u => u.Role)        : query.OrderBy(u => u.Role),
            "isactive"    => isDescending ? query.OrderByDescending(u => u.IsActive)    : query.OrderBy(u => u.IsActive),
            "createddate" => isDescending ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate),
            _ => query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
        };
    }

    #endregion
}
