using ETR.ToDo.Core.Entities.Logging;
using ETR.ToDo.Services.Core.Logging.Dto;
using Riok.Mapperly.Abstractions;

namespace ETR.ToDo.Services.Mapping;

/// <summary>
/// Navigation properties and non-DTO fields are silently skipped via RequiredMappingStrategy.Target.
/// Sensitive infrastructure fields are explicitly excluded so accidental DTO additions cause a compile error.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ApiLogMapper
{
    // CreateDto → Entity
    [MapperIgnoreTarget(nameof(ApiLog.Id))]
    [MapperIgnoreTarget(nameof(ApiLog.CreatedDate))]
    [MapperIgnoreTarget(nameof(ApiLog.CreatedBy))]
    [MapperIgnoreTarget(nameof(ApiLog.ModifiedDate))]
    [MapperIgnoreTarget(nameof(ApiLog.ModifiedBy))]
    [MapperIgnoreTarget(nameof(ApiLog.IsDeleted))]
    [MapperIgnoreTarget(nameof(ApiLog.DeletedDate))]
    [MapperIgnoreTarget(nameof(ApiLog.DeletedBy))]
    public partial ApiLog CreateDtoToApiLog(CreateApiLogDto dto);

    // Entity → DTO
    [MapperIgnoreSource(nameof(ApiLog.IsDeleted))]
    [MapperIgnoreSource(nameof(ApiLog.DeletedDate))]
    [MapperIgnoreSource(nameof(ApiLog.DeletedBy))]
    [MapperIgnoreSource(nameof(ApiLog.CreatedBy))]
    [MapperIgnoreSource(nameof(ApiLog.ModifiedDate))]
    [MapperIgnoreSource(nameof(ApiLog.ModifiedBy))]
    public partial ApiLogDto ApiLogToDto(ApiLog log);

    public partial List<ApiLogDto> ApiLogsToDtos(List<ApiLog> logs);
}
