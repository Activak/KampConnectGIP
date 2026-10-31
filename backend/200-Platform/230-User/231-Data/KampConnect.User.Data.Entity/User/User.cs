using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;
using KampConnect.User.Data.Entity.User;

namespace KampConnect.User.Data.Entity.User;

[Table("usr_user", Schema = "user")]
public class User
{
    #region Properties
    [Key]
    public Guid user_id { get; set; }

    [Required]
    public Guid auth_user_id { get; set; }

    [Required]
    public string first_name { get; set; }

    [Required]  
    public string last_name { get; set; }

    public DateOnly birth_date { get; set; }

    [MaxLength(255)]
    public string preferred_language { get; set; }

    public Guid avatar_file_id { get; set; }

    public UserStatus status { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }


    #endregion

    #region Navigation Properties

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
