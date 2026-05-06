using ETR.ToDo.Core.Entities.Users;
using ETR.ToDo.Services.Core.Users.Dto;
using Riok.Mapperly.Abstractions;

namespace ETR.ToDo.Services.Mapping;

/// <summary>
/// Mapperly source generator for User entity mappings.
/// Navigation properties and non-DTO fields are silently skipped.
/// PasswordHash has an explicit guard — it must never reach any DTO.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class UserMapper
{
    /// <summary>Maps User entity to UserListDto (simple list view)</summary>
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    public partial UserListDto UserToUserListDto(User user);

    /// <summary>Maps collection of User entities to UserListDto collection</summary>
    public partial List<UserListDto> UsersToUserListDtos(List<User> users);

    /// <summary>Maps User entity to UserDto (full details)</summary>
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    [MapProperty(nameof(User.FullName), nameof(UserDto.FullName))]
    public partial UserDto UserToUserDto(User user);

    /// <summary>
    /// Maps CreateUserDto to User entity.
    /// Password will be hashed separately before calling this.
    /// IsSystemUser is always false for new users — set by the service.
    /// </summary>
    [MapProperty(nameof(CreateUserDto.Password), nameof(User.PasswordHash))]
    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.IsSystemUser))]
    [MapperIgnoreTarget(nameof(User.CreatedDate))]
    [MapperIgnoreTarget(nameof(User.CreatedBy))]
    [MapperIgnoreTarget(nameof(User.ModifiedDate))]
    [MapperIgnoreTarget(nameof(User.ModifiedBy))]
    [MapperIgnoreTarget(nameof(User.IsDeleted))]
    [MapperIgnoreTarget(nameof(User.DeletedDate))]
    [MapperIgnoreTarget(nameof(User.DeletedBy))]
    public partial User CreateUserDtoToUser(CreateUserDto dto);

    /// <summary>Maps UpdateUserDto onto an existing User entity (updates only provided fields)</summary>
    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.PasswordHash))]
    [MapperIgnoreTarget(nameof(User.IsSystemUser))]
    [MapperIgnoreTarget(nameof(User.CreatedDate))]
    [MapperIgnoreTarget(nameof(User.CreatedBy))]
    [MapperIgnoreTarget(nameof(User.ModifiedDate))]
    [MapperIgnoreTarget(nameof(User.ModifiedBy))]
    [MapperIgnoreTarget(nameof(User.IsDeleted))]
    [MapperIgnoreTarget(nameof(User.DeletedDate))]
    [MapperIgnoreTarget(nameof(User.DeletedBy))]
    public partial void UpdateUserDtoToUser(UpdateUserDto dto, User user);
}
