using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KampConnect.Shared.Logging.Config.Tools;
using System.Runtime.CompilerServices;
using System.Data.Common;

namespace KampConnect.Camp.Data.Entity.Camps;

[Table("cmp_camp", Schema = "camp")]
public class Camp
{
    #region Properties
    [Key]
    public Guid camp_id { get; set; }

    [Required]
    public Guid tenant_id { get; set; }

    [Required]
    public string name { get; set; } = string.Empty;

    public string? description { get; set; }

    public string? location { get; set; }

    [Required]
    public DateTime start_date { get; set; }

    [Required]
    public DateTime end_date { get; set; }

    public int? age_min { get; set; }

    public int? age_max { get; set; }

    [Required]
    public CampStatus status { get; set; }

    [Required]
    public bool is_published { get; set; }

    public DateTime? published_at { get; set; }

    [Required]
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
