USE FpolyCafeDb;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ingredients') AND name = 'IsActive')
    ALTER TABLE Ingredients ADD IsActive BIT NOT NULL DEFAULT 1;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ingredients') AND name = 'MinStockLevel')
    EXEC sp_rename 'Ingredients.MinStockLevel', 'MinimumStockLevel', 'COLUMN';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ingredients') AND name = 'MinimumStockLevel')
    ALTER TABLE Ingredients ADD MinimumStockLevel DECIMAL(18,2) NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ingredients') AND name = 'LastUnitCost')
    ALTER TABLE Ingredients ADD LastUnitCost DECIMAL(18,2) NOT NULL DEFAULT 0;
GO
