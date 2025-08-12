using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Admin
{
    public int AdminId { get; set; }

    public int? AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Account? Account { get; set; }
}
