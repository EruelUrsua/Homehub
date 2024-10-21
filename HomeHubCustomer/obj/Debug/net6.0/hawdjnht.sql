IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Admin] (
    [AdminID] nvarchar(50) NOT NULL,
    [Email] nvarchar(50) NOT NULL,
    [Password] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Admin] PRIMARY KEY ([AdminID])
);
GO

CREATE TABLE [BugReports] (
    [BugID] nvarchar(50) NOT NULL,
    [UserID] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [FunctionID] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_BugReports] PRIMARY KEY ([BugID])
);
GO

CREATE TABLE [Businesses] (
    [UserID] nvarchar(50) NOT NULL,
    [Email] nvarchar(20) NOT NULL,
    [Password] nvarchar(20) NOT NULL,
    [BusinessName] nvarchar(50) NOT NULL,
    [RepresentativeName] nvarchar(50) NOT NULL,
    [ContactNo] int NOT NULL,
    [OfferList] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Businesses] PRIMARY KEY ([UserID])
);
GO

CREATE TABLE [ClientOrders] (
    [ClientID] nvarchar(50) NOT NULL,
    [BusinessID] nvarchar(50) NOT NULL,
    [OrderDate] datetime NOT NULL DEFAULT ((getdate())),
    [Schedule] datetime NOT NULL DEFAULT ((getdate())),
    [OrderedPS] nvarchar(50) NOT NULL,
    [Fee] money NOT NULL,
    [Status] bit NOT NULL,
    [PromoCode] nvarchar(50) NOT NULL,
    [UserID] nvarchar(50) NOT NULL,
    [RatingID] nvarchar(50) NOT NULL,
    [ReportID] nvarchar(50) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_ClientOrders] PRIMARY KEY ([ClientID])
);
GO

CREATE TABLE [Customers] (
    [UserID] nvarchar(50) NOT NULL,
    [Email] nvarchar(20) NOT NULL,
    [Password] nvarchar(20) NOT NULL,
    [Firstname] nvarchar(20) NOT NULL,
    [Lastname] nvarchar(20) NOT NULL,
    [ContactNo] nvarchar(11) NOT NULL,
    [Address] nvarchar(80) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([UserID])
);
GO

CREATE TABLE [OrdersLogs] (
    [LogID] nvarchar(50) NOT NULL,
    [OrderID] nvarchar(50) NOT NULL,
    [OrderDate] datetime NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [BusinessID] nvarchar(50) NOT NULL,
    [Item] nvarchar(50) NOT NULL,
    [Qty] int NOT NULL,
    [Date] datetime NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_OrdersLogs] PRIMARY KEY ([LogID])
);
GO

CREATE TABLE [Products] (
    [ProductID] nvarchar(50) NOT NULL,
    [ProductItem] nvarchar(50) NOT NULL,
    [Qty] int NOT NULL,
    [Price] money NOT NULL,
    [ContainerType] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductID])
);
GO

CREATE TABLE [Promos] (
    [PromoID] nvarchar(50) NOT NULL,
    [PromoName] nvarchar(10) NOT NULL,
    [PromoCode] nvarchar(10) NOT NULL,
    [PromoStart] date NOT NULL,
    [PromoEnd] date NOT NULL,
    [BusinessName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Promos] PRIMARY KEY ([PromoID])
);
GO

CREATE TABLE [Ratings] (
    [RatingID] nvarchar(50) NOT NULL,
    [UserID] nvarchar(50) NOT NULL,
    [Score] int NOT NULL,
    [Comments] nvarchar(50) NOT NULL,
    [Date] date NOT NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([RatingID])
);
GO

CREATE TABLE [Reports] (
    [ReportID] nvarchar(50) NOT NULL,
    [Date] date NOT NULL,
    [Title] nvarchar(50) NOT NULL,
    [Description] nvarchar(150) NOT NULL,
    [UserID] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([ReportID])
);
GO

CREATE TABLE [Services] (
    [ServiceID] nvarchar(50) NOT NULL,
    [ServiceItem] nvarchar(50) NOT NULL,
    [Details] nvarchar(100) NOT NULL,
    [Fee] money NOT NULL,
    [Available] bit NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceID])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240614051609_InitialCreate', N'7.0.20');
GO

COMMIT;
GO

