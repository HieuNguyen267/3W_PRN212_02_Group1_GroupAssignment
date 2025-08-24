using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class ShoppingCart
{
    public int CartId { get; set; }

    public int CustomerId { get; set; }

    public string ProductId { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Product? Product { get; set; }
}
