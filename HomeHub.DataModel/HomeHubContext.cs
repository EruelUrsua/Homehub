using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.DataModel;

public partial class HomeHubContext : DbContext
{
    public HomeHubContext()
    {
    }

    public HomeHubContext(DbContextOptions<HomeHubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<BugReport> BugReports { get; set; }

    public virtual DbSet<Business> Businesses { get; set; }

    public virtual DbSet<ClientOrder> ClientOrders { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<OrdersLog> OrdersLogs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Promo> Promos { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        =>

        /*
            optionsBuilder.UseSqlServer("Server=DESKTOP-TRU0264\\SQLEXPRESS;Database=HomeHub;Integrated Security=SSPI;TrustServerCertificate=true;");

    /*
            optionsBuilder.UseSqlServer("Server=desktop-1nft1om;" +
          "Database=HomeHub; Integrated Security=SSPI;" +
          "TrustServerCertificate=true");
    */
        optionsBuilder.UseSqlServer("Server=DESKTOP-JJNUTRM\\MSSQL2022;" +
          "Database=HomeHub; Integrated Security=SSPI;" +
          "TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admin");

            entity.Property(e => e.AdminId)
                .HasMaxLength(50)
                .HasColumnName("AdminID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
        });

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
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");
            entity.Property(e => e.BusinessName).HasMaxLength(50);
            entity.Property(e => e.BusinessType).HasMaxLength(1);
            entity.Property(e => e.CompanyAddress).HasMaxLength(100);
            entity.Property(e => e.ContactNo).HasMaxLength(11);
            entity.Property(e => e.Email).HasMaxLength(20);
            entity.Property(e => e.OfferList).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(20);
            entity.Property(e => e.RepresentativeName).HasMaxLength(50);
        });

        modelBuilder.Entity<ClientOrder>(entity =>
        {
            entity.HasKey(e => e.ClientId);

            entity.Property(e => e.ClientId)
                .HasColumnName("ClientID");
            entity.Property(e => e.BusinessId)
                .HasMaxLength(50)
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
            entity.Property(e => e.UserId)
                .HasColumnName("UserID");
            entity.Property(e => e.ModeOfPayment)
                .HasMaxLength(5);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(80);
            entity.Property(e => e.ContactNo).HasMaxLength(11);
            entity.Property(e => e.Email).HasMaxLength(20);
            entity.Property(e => e.Firstname).HasMaxLength(20);
            entity.Property(e => e.Lastname).HasMaxLength(20);
            entity.Property(e => e.Password).HasMaxLength(20);
        });

        modelBuilder.Entity<OrdersLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.Property(e => e.LogId)
                .HasMaxLength(50)
                .HasColumnName("LogID");
            entity.Property(e => e.BusinessId)
                .HasMaxLength(50)
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
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("ProductID");
            entity.Property(e => e.ContainerType).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.ProductItem).HasMaxLength(50);
        });

        modelBuilder.Entity<Promo>(entity =>
        {
            entity.Property(e => e.PromoId).HasColumnName("PromoID");
            entity.Property(e => e.BusinessName).HasMaxLength(50);
            entity.Property(e => e.PromoCode).HasMaxLength(10);
            entity.Property(e => e.PromoEnd).HasColumnType("date");
            entity.Property(e => e.PromoName).HasMaxLength(10);
            entity.Property(e => e.PromoStart).HasColumnType("date");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.Property(e => e.RatingId)
                .HasMaxLength(50)
                .HasColumnName("RatingID");
            entity.Property(e => e.Comments).HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
