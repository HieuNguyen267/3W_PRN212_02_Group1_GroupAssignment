using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public string? ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public virtual Product? Product { get; set; }
}
