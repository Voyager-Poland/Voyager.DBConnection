-- =============================================
-- Database Initialization Script
-- Creates test tables for integration testing
-- =============================================

-- Note: Syntax varies by database
-- This script shows SQL Server syntax
-- Adapt for other databases as needed

-- Create test database (SQL Server)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'testdb')
BEGIN
    CREATE DATABASE testdb;
END
GO

USE testdb;
GO

-- Drop tables if they exist (for clean re-runs)
IF OBJECT_ID('OrderItems', 'U') IS NOT NULL DROP TABLE OrderItems;
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Products', 'U') IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

-- Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Products table
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CategoryId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Orders table
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(18,2) NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Created',
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- OrderItems table
CREATE TABLE OrderItems (
    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- Create stored procedures for testing

-- CreateUser procedure
CREATE OR ALTER PROCEDURE CreateUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @Age INT,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Users (Username, Email, Age)
    VALUES (@Username, @Email, @Age);

    SET @UserId = SCOPE_IDENTITY();
END
GO

-- GetUserById procedure
CREATE OR ALTER PROCEDURE GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UserId, Username, Email, Age, IsActive, CreatedDate
    FROM Users
    WHERE UserId = @UserId;
END
GO

-- CreateOrder procedure
CREATE OR ALTER PROCEDURE CreateOrder
    @UserId INT,
    @TotalAmount DECIMAL(18,2),
    @OrderId INT OUTPUT,
    @OrderNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Generate order number
    SET @OrderNumber = 'ORD' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

    INSERT INTO Orders (UserId, OrderNumber, TotalAmount)
    VALUES (@UserId, @OrderNumber, @TotalAmount);

    SET @OrderId = SCOPE_IDENTITY();
END
GO

-- GetUserCount procedure
CREATE OR ALTER PROCEDURE GetUserCount
    @Active BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Active IS NULL
        SELECT COUNT(*) AS UserCount FROM Users;
    ELSE
        SELECT COUNT(*) AS UserCount FROM Users WHERE IsActive = @Active;
END
GO

-- Insert test data
INSERT INTO Users (Username, Email, Age) VALUES
    ('john_doe', 'john@example.com', 30),
    ('jane_smith', 'jane@example.com', 25),
    ('bob_wilson', 'bob@example.com', 35);
GO

INSERT INTO Products (ProductName, Price, CategoryId) VALUES
    ('Laptop', 999.99, 1),
    ('Mouse', 29.99, 1),
    ('Keyboard', 79.99, 1),
    ('Monitor', 299.99, 1);
GO

PRINT 'Database initialized successfully!';
GO
