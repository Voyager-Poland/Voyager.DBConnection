-- =============================================
-- PostgreSQL Database Initialization Script
-- Creates test tables for integration testing
-- =============================================

-- Drop tables if they exist (for clean re-runs)
DROP TABLE IF EXISTS OrderItems CASCADE;
DROP TABLE IF EXISTS Orders CASCADE;
DROP TABLE IF EXISTS Products CASCADE;
DROP TABLE IF EXISTS Users CASCADE;

-- Users table
CREATE TABLE Users (
    UserId SERIAL PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL,
    Age INTEGER NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Products table
CREATE TABLE Products (
    ProductId SERIAL PRIMARY KEY,
    ProductName VARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CategoryId INTEGER NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Orders table
CREATE TABLE Orders (
    OrderId SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(18,2) NOT NULL,
    OrderDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Created',
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- OrderItems table
CREATE TABLE OrderItems (
    OrderItemId SERIAL PRIMARY KEY,
    OrderId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- Create stored procedures (PostgreSQL uses functions)

-- CreateUser function
CREATE OR REPLACE FUNCTION CreateUser(
    p_Username VARCHAR(50),
    p_Email VARCHAR(100),
    p_Age INTEGER,
    OUT p_UserId INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO Users (Username, Email, Age)
    VALUES (p_Username, p_Email, p_Age)
    RETURNING UserId INTO p_UserId;
END;
$$;

-- GetUserById function
CREATE OR REPLACE FUNCTION GetUserById(p_UserId INTEGER)
RETURNS TABLE (
    UserId INTEGER,
    Username VARCHAR(50),
    Email VARCHAR(100),
    Age INTEGER,
    IsActive BOOLEAN,
    CreatedDate TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT u.UserId, u.Username, u.Email, u.Age, u.IsActive, u.CreatedDate
    FROM Users u
    WHERE u.UserId = p_UserId;
END;
$$;

-- CreateOrder function
CREATE OR REPLACE FUNCTION CreateOrder(
    p_UserId INTEGER,
    p_TotalAmount DECIMAL(18,2),
    OUT p_OrderId INTEGER,
    OUT p_OrderNumber VARCHAR(50)
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Generate order number
    p_OrderNumber := 'ORD' || TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');

    INSERT INTO Orders (UserId, OrderNumber, TotalAmount)
    VALUES (p_UserId, p_OrderNumber, p_TotalAmount)
    RETURNING OrderId INTO p_OrderId;
END;
$$;

-- GetUserCount function
CREATE OR REPLACE FUNCTION GetUserCount(p_Active BOOLEAN DEFAULT NULL)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_Count INTEGER;
BEGIN
    IF p_Active IS NULL THEN
        SELECT COUNT(*) INTO v_Count FROM Users;
    ELSE
        SELECT COUNT(*) INTO v_Count FROM Users WHERE IsActive = p_Active;
    END IF;

    RETURN v_Count;
END;
$$;

-- Insert test data
INSERT INTO Users (Username, Email, Age) VALUES
    ('john_doe', 'john@example.com', 30),
    ('jane_smith', 'jane@example.com', 25),
    ('bob_wilson', 'bob@example.com', 35);

INSERT INTO Products (ProductName, Price, CategoryId) VALUES
    ('Laptop', 999.99, 1),
    ('Mouse', 29.99, 1),
    ('Keyboard', 79.99, 1),
    ('Monitor', 299.99, 1);

-- Print success message
DO $$
BEGIN
    RAISE NOTICE 'Database initialized successfully!';
END $$;
