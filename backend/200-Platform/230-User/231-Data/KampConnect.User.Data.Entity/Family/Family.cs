using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.User.Data.Entity.Family;

[Table("usr_family", Schema = "user")]
public class Family
{
    #region Properties
    [Key]
    public Guid family_id { get; set; }

    [Required]
    [MaxLength(255)]
    public required string name { get; set; }

    public bool invite_enabled { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }

    #endregion

    #region Navigation Properties
    public ICollection<UserFamilyMember> Members { get; set; } = new List<UserFamilyMember>();

    public ICollection<UserFamilyInvite> Invites { get; set; } = new List<UserFamilyInvite>();

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
