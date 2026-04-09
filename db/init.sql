IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Math_DB')
BEGIN
    CREATE DATABASE Math_DB;
END
GO

USE Math_DB;
GO

IF NOT EXISTS (
    SELECT * FROM sysobjects
    WHERE name = 'MathCalculations' AND xtype = 'U'
)
BEGIN
    CREATE TABLE MathCalculations
    (
        CalculationID INT IDENTITY(1,1) PRIMARY KEY,
        FirstNumber DECIMAL(18,2) NULL,
        SecondNumber DECIMAL(18,2) NULL,
        Operation INT NULL,
        Result DECIMAL(18,2) NULL,
        FirebaseUUID VARCHAR(512) NULL
    );
END
GO