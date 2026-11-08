using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.User.Data.Entity.User;

using KampConnect.User.Data.Entity.Family;

[Table("usr_user", Schema = "user")]
public class User
{
    #region Properties
    [Key]
    public Guid user_id { get; set; }

    // Null voor kinderen zonder eigen login
    public Guid? auth_user_id { get; set; }

    [Required]
    public required string first_name { get; set; }

    [Required]  
    public required string last_name { get; set; }

    [MaxLength(255)]
    public DateOnly birth_date { get; set; }

    public required string preferred_language { get; set; }

    public Guid? avatar_file_id { get; set; }

    public UserStatus status { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }

    [Timestamp]
    public uint version { get; set; }   
    #endregion

    #region Navigation Properties
    // Alleen lidmaatschappen van deze gebruiker, niet de goedkeuringen
    [InverseProperty(nameof(UserFamilyMember.User))]
    public ICollection<UserFamilyMember> FamilyMemberships { get; set; } = new List<UserFamilyMember>();

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
