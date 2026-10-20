using Microsoft.EntityFrameworkCore;
using WarrantHedge.API.Models.Entities;

namespace WarrantHedge.API.Data;

/// <summary>
/// 資料表由 database/01_schema.sql 建立（DB First 概念），
/// 這裡只做欄位對應，不使用 Migration 改動資料庫結構。
/// </summary>
public class WarrantDbContext(DbContextOptions<WarrantDbContext> options) : DbContext(options)
{
    public DbSet<WarrantMaster> WarrantMasters => Set<WarrantMaster>();
    public DbSet<WarrantTrialLog> WarrantTrialLogs => Set<WarrantTrialLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WarrantMaster>(e =>
        {
            e.ToTable("Warrant_Master");
            e.HasKey(x => x.WarrantId);

            e.Property(x => x.WarrantId).HasColumnName("Warrant_ID").HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.StrikePrice).HasColumnName("Strike_Price").HasPrecision(18, 4);
            e.Property(x => x.ConversionRatio).HasColumnName("Conversion_Ratio").HasPrecision(18, 4);
            e.Property(x => x.WarrantType).HasColumnName("Warrant_Type").HasMaxLength(4).IsUnicode(false);
            e.Property(x => x.PositionQty).HasColumnName("Position_Qty");
        });

        modelBuilder.Entity<WarrantTrialLog>(e =>
        {
            e.ToTable("Warrant_Trial_Log");
            e.HasKey(x => x.LogId);

            e.Property(x => x.LogId).HasColumnName("Log_ID").ValueGeneratedOnAdd();
            e.Property(x => x.WarrantId).HasColumnName("Warrant_ID").HasMaxLength(10).IsUnicode(false);
            e.Property(x => x.MarketPrice).HasColumnName("Market_Price").HasPrecision(18, 4);
            e.Property(x => x.TheoryPrice).HasColumnName("Theory_Price").HasPrecision(18, 4);
            e.Property(x => x.HedgeQty).HasColumnName("Hedge_Qty").HasPrecision(18, 2);
            e.Property(x => x.CreatedTime).HasColumnName("Created_Time")
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()")   // 時間以 DB 伺服器為準，不信任 Client 時間
                .ValueGeneratedOnAdd();
            e.Property(x => x.RequestId).HasColumnName("Request_ID");

            e.HasIndex(x => x.RequestId).IsUnique();

            e.HasOne(x => x.Warrant)
             .WithMany(w => w.TrialLogs)
             .HasForeignKey(x => x.WarrantId);
        });
    }
}
