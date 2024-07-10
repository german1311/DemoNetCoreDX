using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;

namespace DXMvcCore.WebApi.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891.
public class DXMvcCoreContextInitializer : DbContextTypesInfoInitializerBase {
	protected override DbContext CreateDbContext() {
		var optionsBuilder = new DbContextOptionsBuilder<DXMvcCoreEFCoreDbContext>()
            .UseSqlServer(";")
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new DXMvcCoreEFCoreDbContext(optionsBuilder.Options);
	}
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class DXMvcCoreDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DXMvcCoreEFCoreDbContext> {
	public DXMvcCoreEFCoreDbContext CreateDbContext(string[] args) {
		throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
		//var optionsBuilder = new DbContextOptionsBuilder<DXMvcCoreEFCoreDbContext>();
		//optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=DXMvcCore");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
		//return new DXMvcCoreEFCoreDbContext(optionsBuilder.Options);
	}
}
[TypesInfoInitializer(typeof(DXMvcCoreContextInitializer))]
public class DXMvcCoreEFCoreDbContext : DbContext {
	public DXMvcCoreEFCoreDbContext(DbContextOptions<DXMvcCoreEFCoreDbContext> options) : base(options) {
	}
	public DbSet<FileData> FileData { get; set; }
	public DbSet<ReportDataV2> ReportDataV2 { get; set; }
    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.AuditItems)
            .WithOne(p => p.AuditedObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.OldItems)
            .WithOne(p => p.OldObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.NewItems)
            .WithOne(p => p.NewObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.UserItems)
            .WithOne(p => p.UserObject);
    }
}

public class DXMvcCoreAuditingDbContext : DbContext {
    public DXMvcCoreAuditingDbContext(DbContextOptions<DXMvcCoreAuditingDbContext> options) : base(options) {
    }
    public DbSet<AuditDataItemPersistent> AuditData { get; set; }
    public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReference { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.AuditItems)
            .WithOne(p => p.AuditedObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.OldItems)
            .WithOne(p => p.OldObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.NewItems)
            .WithOne(p => p.NewObject);
        modelBuilder.Entity<AuditEFCoreWeakReference>()
            .HasMany(p => p.UserItems)
            .WithOne(p => p.UserObject);
    }
}
