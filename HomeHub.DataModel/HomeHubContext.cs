using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.DataModel;

public class HomeHubContext : IdentityDbContext<ApplicationUser>
{

    public HomeHubContext(DbContextOptions options) : base(options)
    {

    }

   // public DbSet<Admin> Admins { get; set; }

    //public DbSet<BugReport> BugReports { get; set; }

   // public DbSet<Business> Businesses { get; set; }

    public DbSet<ClientOrder> ClientOrders { get; set; }

   // public  DbSet<Customer> Customers { get; set; }

    public  DbSet<OrdersLog> OrdersLogs { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Promo> Promos { get; set; }

    public DbSet<Rating> Ratings { get; set; }

    public DbSet<Report> Reports { get; set; }

    public DbSet<Service> Services { get; set; }

    public DbSet<RefundRequest> RefundRequests { get; set; }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Provider> Providers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlServer("Server=DESKTOP-TRU0264\\SQLEXPRESS;Database=HomeHub;Integrated Security=SSPI;TrustServerCertificate=true;");


      //  optionsBuilder.UseSqlServer("Server=DESKTOP-HGGKL34\\SQLEXPRESS;" +
      //"Database=HomeHub; Integrated Security=SSPI;" +
      //"TrustServerCertificate=true");

        optionsBuilder.UseSqlServer("Server=DESKTOP-JJNUTRM\\MSSQL2022;" +
              "Database=HomeHub; Integrated Security=SSPI;" +
              "TrustServerCertificate=true");

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        //modelBuilder.Entity<Admin>(entity =>
        //{
        //    entity.ToTable("Admin");

        //    entity.Property(e => e.AdminId)
        //        .HasMaxLength(50)
        //        .HasColumnName("AdminID");
        //    entity.Property(e => e.Email).HasMaxLength(50);
        //    entity.Property(e => e.Password).HasMaxLength(50);
        //});

        modelBuilder.Entity<BugReport>(entity =>
        {
            entity.HasKey(e => e.BugId);

            entity.Property(e => e.BugId)
                .HasMaxLength(50)
                .HasColumnName("BugID");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.FunctionId)
                .HasMaxLength(50)
                .HasColumnName("FunctionID");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("UserID");
        });



        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("Business");
            entity.HasKey(e => e.UserID);

            entity.Property(e => e.UserID)
                .HasColumnName("UserID").HasMaxLength(450);
            entity.Property(e => e.BusinessName).HasMaxLength(50);
            entity.Property(e => e.Businesstype).HasMaxLength(1);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.BusinessPermit).HasColumnType("nvarchar(MAX)");
        });


        //modelBuilder.Entity<Business>(entity =>
        //{
        //    entity.HasKey(e => e.UserID);

        //    entity.Property(e => e.UserID)
        //        .HasColumnName("UserID");
        //    entity.Property(e => e.BusinessName).HasMaxLength(50);
        //    //entity.Property(e => e.BusinessType).HasMaxLength(1);
        //    entity.Property(e => e.CompanyAddress).HasMaxLength(100);
        //    entity.Property(e => e.ContactNo).HasMaxLength(11);
        //    entity.Property(e => e.Email).HasMaxLength(40);
        //    entity.Property(e => e.OfferList).HasMaxLength(50);
        //    entity.Property(e => e.Password).HasMaxLength(20);
        //    entity.Property(e => e.RepresentativeName).HasMaxLength(50);
        //    entity.Property(e => e.BusinessPermitNo).HasMaxLength(50);
        //});

        modelBuilder.Entity<ClientOrder>(entity =>
        {
            entity.HasKey(e => e.ClientId);

            entity.Property(e => e.ClientId)
                .HasColumnName("ClientID");
            entity.Property(e => e.BusinessId)
                .HasColumnType("nvarchar(450)")
                .HasColumnName("BusinessID");
            entity.Property(e => e.Fee).HasColumnType("money");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderedPs)
                .HasMaxLength(50)
                .HasColumnName("OrderedPS");
            entity.Property(e => e.PromoCode).HasMaxLength(50);
            entity.Property(e => e.RatingId)
                .HasColumnName("RatingID");
            entity.Property(e => e.ReportId)
                .HasColumnName("ReportID");
            entity.Property(e => e.Schedule)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasDefaultValue("Pending")
                .HasColumnType("nvarchar(20)")
                .HasMaxLength(10);
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("UserID");
            entity.Property(e => e.ModeOfPayment)
                .HasMaxLength(5);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("FirstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("LastName");
            entity.Property(e => e.AddInstructions)
                .HasMaxLength(150);
        });

        //modelBuilder.Entity<Customer>(entity =>
        //{
        //    entity.HasKey(e => e.UserId);

        //    entity.Property(e => e.UserId)
        //        .HasColumnName("UserID");
        //    entity.Property(e => e.Address).HasMaxLength(80);
        //    entity.Property(e => e.ContactNo).HasMaxLength(11);
        //    entity.Property(e => e.Email).HasMaxLength(20);
        //    entity.Property(e => e.Firstname).HasMaxLength(20);
        //    entity.Property(e => e.Lastname).HasMaxLength(20);
        //    entity.Property(e => e.Password).HasMaxLength(20);
        //    entity.Property(e => e.ValidIDno).HasMaxLength(50);
        //});

        modelBuilder.Entity<OrdersLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.Property(e => e.LogId)
                .HasMaxLength(50)
                .HasColumnName("LogID");
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.BusinessId)
                .HasColumnType("nvarchar(450)")
                .HasColumnName("BusinessID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Item).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .HasColumnName("OrderID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Fee)
                .HasColumnType("MONEY")
                .IsRequired();
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .HasColumnName("PromoCode");
            entity.Property(e => e.PayStatus)
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("ProductID");
            entity.Property(e => e.ContainerType).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.ProductItem).HasMaxLength(50);
            entity.Property(e => e.ProviderID).HasColumnType("nvarchar(450)");
            entity.Property(e => e.ProductImagePath)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("ProductImagePath");
        });

        modelBuilder.Entity<Promo>(entity =>
        {
            entity.Property(e => e.PromoId).HasColumnName("PromoID");
            entity.Property(e => e.BusinessName).HasMaxLength(50);
            entity.Property(e => e.PromoCode).HasMaxLength(10);
            entity.Property(e => e.PromoEnd).HasColumnType("date");
            entity.Property(e => e.PromoName).HasMaxLength(10);
            entity.Property(e => e.PromoStart).HasColumnType("date");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,4)");
            entity.Property(n => n.BusinessId)
                .HasColumnType("nvarchar(450)");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.Property(e => e.RatingId)
                .HasColumnName("RatingID");
            entity.Property(e => e.Comments).HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .HasColumnName("OrderID");
            entity.Property(e => e.BusinessId)
                .HasColumnName("BusinessID")
                .HasColumnType("nvarchar(450)");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.Property(e => e.ReportId)
                .HasMaxLength(50)
                .HasColumnName("ReportID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.ServiceId)
                .HasMaxLength(50)
                .HasColumnName("ServiceID");
            entity.Property(e => e.Details).HasMaxLength(100);
            entity.Property(e => e.Fee).HasColumnType("money");
            entity.Property(e => e.ServiceItem).HasMaxLength(50);
            entity.Property(e => e.ProviderID).HasColumnType("nvarchar(450)");
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.ToTable("RefundRequests");

            entity.Property(e => e.RefundId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RefundID");

            entity.Property(e => e.OrderId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("OrderID");

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.BusinessId)
                .IsRequired()
                .HasColumnType("nvarchar(MAX)")
                .HasColumnName("BusinessID");

            entity.Property(e => e.Item)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.RefundQuantity)
                .IsRequired();

            entity.Property(e => e.RefundReason)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.RefundStatus)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.RefundRequestDate)
                .IsRequired()
                .HasColumnType("DATETIME");

            entity.Property(e => e.RefundActionDate)
                .HasColumnType("DATETIME");

            entity.Property(e => e.Fee)
                .IsRequired()
                .HasColumnType("MONEY");

            entity.Property(e => e.PromoCode)
                .HasMaxLength(50);

            entity.Property(e => e.RefundAmount)
                .HasColumnType("MONEY");

            entity.Property(r => r.RejectionReason)
                .HasMaxLength(500)  
                .IsUnicode(true);  // Allow special characters if needed
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.BusinessId)
                .HasColumnType("nvarchar(450)");
            entity.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(255);  

            entity.Property(n => n.IsRead)
                .HasDefaultValue(false);  // Default unread

            entity.Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETDATE()");  // Default to current timestamp
        });

        //        OnModelCreatingPartial(modelBuilder);
    }

  //  partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
