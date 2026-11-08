using KampConnect.User.Business.Contracts.ClassTypes.Dtos.Family;
 namespace KampConnect.User.Business.Contracts.ClassTypes.Results;

public enum FamilyError
{
    None = 0,
    UserNotFound = 1,
    FamilyNotFound = 2,
    MemberNotFound = 3, 
    InviteNotFound = 4,
    NotAuthorized = 5,
    AlreadyMember = 6,
    InvalidOrExpiredCode = 7,
    InvitesDisabled = 8,
    NotPending = 9,
    LastGuardian = 10,
    ConcurrencyConflict = 11,
    ValidationFailed = 12
}

public record FamilyResult(
    bool Succeeded,
    FamilyError Error = FamilyError.None,
    FamilyDto? Family = null,
    List<FamilyDto>? Families = null,
    FamilyMemberDto? Member = null,
    List<FamilyMemberDto>? Members = null,
    List<FamilyInviteDto>? Invites = null,
    CreatedInviteDto? CreatedInvite = null,
    IDictionary<string, string[]>? ValidationErrors = null)
{
    public static FamilyResult Ok(
        FamilyDto? family = null,
        List<FamilyDto>? families = null,
        FamilyMemberDto? member = null,
        List<FamilyMemberDto>? members = null,
        List<FamilyInviteDto>? invites = null,
        CreatedInviteDto? createdInvite = null) => new(true, Family: family, Families: families, Member: member, Members: members, Invites: invites, CreatedInvite: createdInvite);
    public static FamilyResult Fail(FamilyError error) => new(false, error);
    public static FamilyResult Invalid(IDictionary<string, string[]> errors) => new(false, FamilyError.ValidationFailed, ValidationErrors: errors);
}
