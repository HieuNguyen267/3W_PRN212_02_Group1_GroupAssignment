-- Update Order Status to support "Cancelled"
USE ShoppingOnline;
GO

-- Drop existing CHECK constraint
ALTER TABLE Orders DROP CONSTRAINT CK__Orders__Status__5FB337D6;
GO

-- Add new CHECK constraint with "Cancelled" status
ALTER TABLE Orders ADD CONSTRAINT CK_Orders_Status 
CHECK (Status IN ('Pending', 'Confirmed', 'Preparing', 'Shipping', 'Delivered', 'Completed', 'Cancelled'));
GO

PRINT 'Updated Orders table to support Cancelled status';

