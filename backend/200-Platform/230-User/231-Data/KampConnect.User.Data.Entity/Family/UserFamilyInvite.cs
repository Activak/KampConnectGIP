using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.User.Data.Entity.Family;

using User = KampConnect.User.Data.Entity.User.User;

[Table("usr_family_invite", Schema = "user")]
public class UserFamilyInvite
{
    #region Properties
    [Key]
    public int family_invite_id { get; set; }

    [Required]
    public required Guid family_id { get; set; }

    [Required]
    public required byte[] code_hash { get; set; }

    [Required]
    public Guid created_by_user_id { get; set; }

    [Required]
    public DateTime expires_at { get; set; }

    public DateTime? revoked_at { get; set; }

    public int? used_count { get; set; }

    public DateTime created_at { get; set; }
    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(family_id))]
    public Family? Family { get; set; }

    [ForeignKey(nameof(created_by_user_id))]
    public User? CreatedBy { get; set; }

    [InverseProperty(nameof(UserFamilyMember.Invite))]
    public ICollection<UserFamilyMember> Members { get; set; } = new List<UserFamilyMember>();

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
