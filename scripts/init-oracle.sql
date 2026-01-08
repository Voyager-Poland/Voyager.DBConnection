-- =============================================
-- Oracle Database Initialization Script
-- Creates test tables for integration testing
-- =============================================

-- Connect to FREEPDB1 (for Oracle 23ai FREE)
-- This ensures we're in the correct pluggable database
ALTER SESSION SET CONTAINER = FREEPDB1;

-- Drop tables if they exist (for clean re-runs)
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE OrderItems CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE Orders CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE Products CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE Users CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN NULL;
END;
/

-- Users table
CREATE TABLE Users (
    UserId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Username VARCHAR2(50) NOT NULL UNIQUE,
    Email VARCHAR2(100) NOT NULL,
    Age NUMBER NOT NULL,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CreatedDate TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
);

-- Products table
CREATE TABLE Products (
    ProductId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ProductName VARCHAR2(100) NOT NULL,
    Price NUMBER(18,2) NOT NULL,
    CategoryId NUMBER NOT NULL,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CreatedDate TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
);

-- Orders table
CREATE TABLE Orders (
    OrderId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    UserId NUMBER NOT NULL,
    OrderNumber VARCHAR2(50) NOT NULL UNIQUE,
    TotalAmount NUMBER(18,2) NOT NULL,
    OrderDate TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    Status VARCHAR2(20) DEFAULT 'Created' NOT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- OrderItems table
CREATE TABLE OrderItems (
    OrderItemId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    OrderId NUMBER NOT NULL,
    ProductId NUMBER NOT NULL,
    Quantity NUMBER NOT NULL,
    Price NUMBER(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- Create stored procedures

-- CreateUser procedure
CREATE OR REPLACE PROCEDURE CreateUser(
    p_Username IN VARCHAR2,
    p_Email IN VARCHAR2,
    p_Age IN NUMBER,
    p_UserId OUT NUMBER
)
AS
BEGIN
    INSERT INTO Users (Username, Email, Age)
    VALUES (p_Username, p_Email, p_Age)
    RETURNING UserId INTO p_UserId;

    COMMIT;
END;
/

-- GetUserById procedure
CREATE OR REPLACE PROCEDURE GetUserById(
    p_UserId IN NUMBER,
    p_Cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_Cursor FOR
    SELECT UserId, Username, Email, Age, IsActive, CreatedDate
    FROM Users
    WHERE UserId = p_UserId;
END;
/

-- CreateOrder procedure
CREATE OR REPLACE PROCEDURE CreateOrder(
    p_UserId IN NUMBER,
    p_TotalAmount IN NUMBER,
    p_OrderId OUT NUMBER,
    p_OrderNumber OUT VARCHAR2
)
AS
BEGIN
    -- Generate order number
    p_OrderNumber := 'ORD' || TO_CHAR(SYSTIMESTAMP, 'YYYYMMDDHH24MISS');

    INSERT INTO Orders (UserId, OrderNumber, TotalAmount)
    VALUES (p_UserId, p_OrderNumber, p_TotalAmount)
    RETURNING OrderId INTO p_OrderId;

    COMMIT;
END;
/

-- GetUserCount procedure
CREATE OR REPLACE PROCEDURE GetUserCount(
    p_Active IN NUMBER DEFAULT NULL,
    p_Count OUT NUMBER
)
AS
BEGIN
    IF p_Active IS NULL THEN
        SELECT COUNT(*) INTO p_Count FROM Users;
    ELSE
        SELECT COUNT(*) INTO p_Count FROM Users WHERE IsActive = p_Active;
    END IF;
END;
/

-- Insert test data
INSERT INTO Users (Username, Email, Age) VALUES ('john_doe', 'john@example.com', 30);
INSERT INTO Users (Username, Email, Age) VALUES ('jane_smith', 'jane@example.com', 25);
INSERT INTO Users (Username, Email, Age) VALUES ('bob_wilson', 'bob@example.com', 35);

INSERT INTO Products (ProductName, Price, CategoryId) VALUES ('Laptop', 999.99, 1);
INSERT INTO Products (ProductName, Price, CategoryId) VALUES ('Mouse', 29.99, 1);
INSERT INTO Products (ProductName, Price, CategoryId) VALUES ('Keyboard', 79.99, 1);
INSERT INTO Products (ProductName, Price, CategoryId) VALUES ('Monitor', 299.99, 1);

COMMIT;

-- Print success message
BEGIN
    DBMS_OUTPUT.PUT_LINE('Database initialized successfully for ' || USER || '!');
END;
/
