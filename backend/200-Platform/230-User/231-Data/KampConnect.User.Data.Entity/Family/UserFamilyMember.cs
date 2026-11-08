using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;


namespace KampConnect.User.Data.Entity.Family;

using User = KampConnect.User.Data.Entity.User.User;

[Table("usr_family_member", Schema = "user")]
public class UserFamilyMember
{
    #region Properties
    [Key]
    public int family_member_id { get; set; }

    [Required]
    public required Guid family_id { get; set; }

    [Required]
    public required Guid user_id { get; set; }

    [Required]
    public required FamilyRoleType role_type { get; set; }

    [Required]
    public required FamilyMemberStatus status { get; set; }

    public Guid? approved_by_user_id { get; set; }

    [Required]
    public required bool is_primary_contact { get; set; }

    public int? invite_id { get; set; }

    public DateTime? requested_at { get; set; }

    public DateTime? approved_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }

    [Timestamp]
    public uint version { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(family_id))]
    public Family? Family { get; set; }

    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    [ForeignKey(nameof(approved_by_user_id))]
    public User? ApprovedBy { get; set; }

    [ForeignKey(nameof(invite_id))]
    public UserFamilyInvite? Invite { get; set; }

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
