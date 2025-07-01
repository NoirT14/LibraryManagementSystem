using System;
using System.Collections.Generic;
using APIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Data;

public partial class LibraryDatabaseContext : DbContext
{
    public LibraryDatabaseContext()
    {
    }

    public LibraryDatabaseContext(DbContextOptions<LibraryDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<BookVariant> BookVariants { get; set; }

    public virtual DbSet<BookVolume> BookVolumes { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CoverType> CoverTypes { get; set; }

    public virtual DbSet<Edition> Editions { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<PaperQuality> PaperQualities { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId).HasName("PK__authors__86516BCFD354C6E2");

            entity.ToTable("authors");

            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.AuthorName)
                .HasMaxLength(100)
                .HasColumnName("author_name");
            entity.Property(e => e.Bio).HasColumnName("bio");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__books__490D1AE13A16BDC5");

            entity.ToTable("books");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.BookStatus)
                .HasMaxLength(50)
                .HasColumnName("book_status");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasColumnName("language");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__books__category___68487DD7");

            entity.HasMany(d => d.Authors).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "BooksAuthor",
                    r => r.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__books_aut__autho__6C190EBB"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__books_aut__book___6B24EA82"),
                    j =>
                    {
                        j.HasKey("BookId", "AuthorId").HasName("PK__books_au__A1680C5D12374835");
                        j.ToTable("books_authors");
                        j.IndexerProperty<int>("BookId").HasColumnName("book_id");
                        j.IndexerProperty<int>("AuthorId").HasColumnName("author_id");
                    });
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.CopyId).HasName("PK__book_cop__3C21D2D29217002E");

            entity.ToTable("book_copies");

            entity.HasIndex(e => e.Barcode, "UQ__book_cop__C16E36F898A42496").IsUnique();

            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .HasColumnName("barcode");
            entity.Property(e => e.CopyStatus)
                .HasMaxLength(50)
                .HasColumnName("copy_status");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Variant).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__book_copi__varia__04E4BC85");
        });

        modelBuilder.Entity<BookVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__book_var__EACC68B795922150");

            entity.ToTable("book_variants");

            entity.Property(e => e.VariantId).HasColumnName("variant_id");
            entity.Property(e => e.CoverTypeId).HasColumnName("cover_type_id");
            entity.Property(e => e.EditionId).HasColumnName("edition_id");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PaperQualityId).HasColumnName("paper_quality_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.PublicationYear).HasColumnName("publication_year");
            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.VolumeId).HasColumnName("volume_id");

            entity.HasOne(d => d.CoverType).WithMany(p => p.BookVariants)
                .HasForeignKey(d => d.CoverTypeId)
                .HasConstraintName("FK__book_vari__cover__00200768");

            entity.HasOne(d => d.Edition).WithMany(p => p.BookVariants)
                .HasForeignKey(d => d.EditionId)
                .HasConstraintName("FK__book_vari__editi__7F2BE32F");

            entity.HasOne(d => d.PaperQuality).WithMany(p => p.BookVariants)
                .HasForeignKey(d => d.PaperQualityId)
                .HasConstraintName("FK__book_vari__paper__01142BA1");

            entity.HasOne(d => d.Publisher).WithMany(p => p.BookVariants)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK__book_vari__publi__7E37BEF6");

            entity.HasOne(d => d.Volume).WithMany(p => p.BookVariants)
                .HasForeignKey(d => d.VolumeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__book_vari__volum__7D439ABD");
        });

        modelBuilder.Entity<BookVolume>(entity =>
        {
            entity.HasKey(e => e.VolumeId).HasName("PK__book_vol__C4AD0FB67506E8B5");

            entity.ToTable("book_volumes");

            entity.Property(e => e.VolumeId).HasColumnName("volume_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.VolumeNumber).HasColumnName("volume_number");
            entity.Property(e => e.VolumeTitle)
                .HasMaxLength(255)
                .HasColumnName("volume_title");

            entity.HasOne(d => d.Book).WithMany(p => p.BookVolumes)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__book_volu__book___7A672E12");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__categori__D54EE9B4DBA883A6");

            entity.ToTable("categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
        });

        modelBuilder.Entity<CoverType>(entity =>
        {
            entity.HasKey(e => e.CoverTypeId).HasName("PK__cover_ty__73A69DF88E50659A");

            entity.ToTable("cover_types");

            entity.HasIndex(e => e.CoverTypeName, "UQ__cover_ty__6648E52014B1500B").IsUnique();

            entity.Property(e => e.CoverTypeId).HasColumnName("cover_type_id");
            entity.Property(e => e.CoverTypeName)
                .HasMaxLength(100)
                .HasColumnName("cover_type_name");
        });

        modelBuilder.Entity<Edition>(entity =>
        {
            entity.HasKey(e => e.EditionId).HasName("PK__editions__8A5478AAC5257E6D");

            entity.ToTable("editions");

            entity.HasIndex(e => e.EditionName, "UQ__editions__A248E477A0629C67").IsUnique();

            entity.Property(e => e.EditionId).HasColumnName("edition_id");
            entity.Property(e => e.EditionName)
                .HasMaxLength(100)
                .HasColumnName("edition_name");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("PK__loans__A1F795544EA4B826");

            entity.ToTable("loans");

            entity.Property(e => e.LoanId).HasColumnName("loan_id");
            entity.Property(e => e.BorrowDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("borrow_date");
            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.Extended)
                .HasDefaultValue(false)
                .HasColumnName("extended");
            entity.Property(e => e.FineAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("fine_amount");
            entity.Property(e => e.LoanStatus)
                .HasMaxLength(50)
                .HasColumnName("loan_status");
            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
            entity.Property(e => e.ReturnDate)
                .HasColumnType("datetime")
                .HasColumnName("return_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Copy).WithMany(p => p.Loans)
                .HasForeignKey(d => d.CopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__loans__copy_id__0F624AF8");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Loans)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK__loans__reservati__1332DBDC");

            entity.HasOne(d => d.User).WithMany(p => p.Loans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__loans__user_id__0E6E26BF");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__notifica__E059842FEEC59944");

            entity.ToTable("notifications");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ForStaff)
                .HasDefaultValue(false)
                .HasColumnName("for_staff");
            entity.Property(e => e.HandledAt)
                .HasColumnType("datetime")
                .HasColumnName("handled_at");
            entity.Property(e => e.HandledBy).HasColumnName("handled_by");
            entity.Property(e => e.HandledStatus)
                .HasDefaultValue(false)
                .HasColumnName("handled_status");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NotificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("notification_date");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadStatus)
                .HasDefaultValue(false)
                .HasColumnName("read_status");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.RelatedId).HasColumnName("related_id");
            entity.Property(e => e.RelatedTable)
                .HasMaxLength(50)
                .HasColumnName("related_table");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderType)
                .HasMaxLength(20)
                .HasDefaultValue("System")
                .HasColumnName("sender_type");

            entity.HasOne(d => d.HandledByNavigation).WithMany(p => p.NotificationHandledByNavigations)
                .HasForeignKey(d => d.HandledBy)
                .HasConstraintName("FK__notificat__handl__1BC821DD");

            entity.HasOne(d => d.Receiver).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__notificat__recei__17036CC0");
        });

        modelBuilder.Entity<PaperQuality>(entity =>
        {
            entity.HasKey(e => e.PaperQualityId).HasName("PK__paper_qu__4A76F090B058DB03");

            entity.ToTable("paper_qualities");

            entity.HasIndex(e => e.PaperQualityName, "UQ__paper_qu__A6AF2EC2D8894A07").IsUnique();

            entity.Property(e => e.PaperQualityId).HasColumnName("paper_quality_id");
            entity.Property(e => e.PaperQualityName)
                .HasMaxLength(100)
                .HasColumnName("paper_quality_name");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("PK__publishe__3263F29DA07472D6");

            entity.ToTable("publishers");

            entity.HasIndex(e => e.PublisherName, "UQ__publishe__8DBCD4124609E5C4").IsUnique();

            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.PublisherName)
                .HasMaxLength(100)
                .HasColumnName("publisher_name");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__reservat__31384C29D160F18A");

            entity.ToTable("reservations");

            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("expiration_date");
            entity.Property(e => e.FulfilledCopyId).HasColumnName("fulfilled_copy_id");
            entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
            entity.Property(e => e.ReservationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("reservation_date");
            entity.Property(e => e.ReservationStatus)
                .HasMaxLength(50)
                .HasColumnName("reservation_status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.FulfilledCopy).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.FulfilledCopyId)
                .HasConstraintName("FK__reservati__fulfi__0A9D95DB");

            entity.HasOne(d => d.ProcessedByNavigation).WithMany(p => p.ReservationProcessedByNavigations)
                .HasForeignKey(d => d.ProcessedBy)
                .HasConstraintName("FK__reservati__proce__0B91BA14");

            entity.HasOne(d => d.User).WithMany(p => p.ReservationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservati__user___07C12930");

            entity.HasOne(d => d.Variant).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__reservati__varia__08B54D69");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC314B419E");

            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F51406F1B");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC5726F1E16D2").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_date");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__5FB337D6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
