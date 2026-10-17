namespace Api.Contracts;

public sealed class CreateGroupInput
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class UpdateGroupInput
{
    public Guid UniqueKey { get; set; }
    public bool Active { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class AddGroupMemberInput
{
    public string Phone { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public Guid GroupUniqueKey { get; set; }
}

public sealed class ArchiveGroupInput
{
    public Guid GroupKey { get; set; }
}
