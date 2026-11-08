using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;

namespace KampConnect.User.Data.Entity.Address;

using User = KampConnect.User.Data.Entity.User.User;


[Table("usr_user_address", Schema = "user")]
public class UserAddress
{
    #region Properties
    [Key]
    public Guid user_address_id { get; set; }

    [Required]
    public Guid user_id { get; set; }

    public AddressType address_type { get; set; }

    public string? street { get; set; }

    public string? house_number { get; set; }

    public string? box { get; set; }

    public string? postal_code { get; set; }

    public string? city { get; set; }

    public string? country_code { get; set; }

    public bool is_primary { get; set; }

    public bool is_validated { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }


    #endregion

    #region Navigation Properties
    [ForeignKey(nameof(user_id))]
    public User? User { get; set; }

    #endregion

    #region ToString

    public override string ToString()
    {
        return this.DumpObject();
    }
    #endregion
}
