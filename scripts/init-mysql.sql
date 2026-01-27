-- =============================================
-- MySQL Database Initialization Script
-- Creates test tables for integration testing
-- =============================================

USE testdb;

-- Drop tables if they exist (for clean re-runs)
DROP TABLE IF EXISTS OrderItems;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Users;

-- Users table
CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Products table
CREATE TABLE Products (
    ProductId INT AUTO_INCREMENT PRIMARY KEY,
    ProductName VARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CategoryId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Orders table
CREATE TABLE Orders (
    OrderId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    OrderNumber VARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(18,2) NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Created',
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- OrderItems table
CREATE TABLE OrderItems (
    OrderItemId INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- Create stored procedures

DELIMITER //

-- CreateUser procedure
CREATE PROCEDURE CreateUser(
    IN p_Username VARCHAR(50),
    IN p_Email VARCHAR(100),
    IN p_Age INT,
    OUT p_UserId INT
)
BEGIN
    INSERT INTO Users (Username, Email, Age)
    VALUES (p_Username, p_Email, p_Age);

    SET p_UserId = LAST_INSERT_ID();
END//

-- GetUserById procedure
CREATE PROCEDURE GetUserById(IN p_UserId INT)
BEGIN
    SELECT UserId, Username, Email, Age, IsActive, CreatedDate
    FROM Users
    WHERE UserId = p_UserId;
END//

-- CreateOrder procedure
CREATE PROCEDURE CreateOrder(
    IN p_UserId INT,
    IN p_TotalAmount DECIMAL(18,2),
    OUT p_OrderId INT,
    OUT p_OrderNumber VARCHAR(50)
)
BEGIN
    -- Generate order number
    SET p_OrderNumber = CONCAT('ORD', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'));

    INSERT INTO Orders (UserId, OrderNumber, TotalAmount)
    VALUES (p_UserId, p_OrderNumber, p_TotalAmount);

    SET p_OrderId = LAST_INSERT_ID();
END//

-- GetUserCount procedure
CREATE PROCEDURE GetUserCount(IN p_Active BOOLEAN)
BEGIN
    IF p_Active IS NULL THEN
        SELECT COUNT(*) AS UserCount FROM Users;
    ELSE
        SELECT COUNT(*) AS UserCount FROM Users WHERE IsActive = p_Active;
    END IF;
END//

DELIMITER ;

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

SELECT 'Database initialized successfully!' AS Message;
