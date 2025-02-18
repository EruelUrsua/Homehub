using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeHub.DataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    AdminID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.AdminID);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BugReports",
                columns: table => new
                {
                    BugID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FunctionID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BugReports", x => x.BugID);
                });

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RepresentativeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    OfferList = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Businesstype = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessPermitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "ClientOrders",
                columns: table => new
                {
                    ClientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Schedule = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    OrderedPS = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fee = table.Column<decimal>(type: "money", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RatingID = table.Column<int>(type: "int", nullable: false),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ModeOfPayment = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOrders", x => x.ClientID);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Firstname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ValidIDno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "OrdersLogs",
                columns: table => new
                {
                    LogID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Item = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fee = table.Column<decimal>(type: "MONEY", nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdersLogs", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    ContainerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ProductImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "Promos",
                columns: table => new
                {
                    PromoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PromoStart = table.Column<DateTime>(type: "date", nullable: false),
                    PromoEnd = table.Column<DateTime>(type: "date", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promos", x => x.PromoID);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    BusinessID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingID);
                });

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    RefundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientID = table.Column<int>(type: "int", nullable: true),
                    BusinessID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Item = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RefundQuantity = table.Column<int>(type: "int", nullable: false),
                    RefundReason = table.Column<string>(type: "TEXT", nullable: false),
                    RefundStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RefundRequestDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    RefundActionDate = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    Fee = table.Column<decimal>(type: "MONEY", nullable: false),
                    PromoCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefundAmount = table.Column<decimal>(type: "MONEY", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.RefundID);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportID);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServiceItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fee = table.Column<decimal>(type: "money", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "BugReports");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "ClientOrders");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrdersLogs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Promos");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
