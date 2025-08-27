-- Add "Cancelled" status to Orders table
USE ShoppingOnline;
GO

-- First, let's see what constraint exists
SELECT CONSTRAINT_NAME, CHECK_CLAUSE 
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS 
WHERE CONSTRAINT_SCHEMA = 'dbo' AND TABLE_NAME = 'Orders';
GO

-- Drop the existing constraint (replace with actual constraint name if different)
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK__Orders__Status__5FB337D6')
BEGIN
    ALTER TABLE Orders DROP CONSTRAINT CK__Orders__Status__5FB337D6;
END
GO

-- Add new constraint with "Cancelled" status
ALTER TABLE Orders ADD CONSTRAINT CK_Orders_Status 
CHECK (Status IN ('Pending', 'Confirmed', 'Preparing', 'Shipping', 'Delivered', 'Completed', 'Cancelled'));
GO

PRINT 'Successfully updated Orders table to support Cancelled status';

