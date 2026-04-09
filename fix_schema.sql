ALTER TABLE Ingredients ADD IsActive BIT NOT NULL DEFAULT 1;
GO
EXEC sp_rename 'Ingredients.MinimumStockLevel', 'MinStockLevel', 'COLUMN';
GO
EXEC sp_rename 'InventoryReceipts.SupplierName', 'Supplier', 'COLUMN';
GO
