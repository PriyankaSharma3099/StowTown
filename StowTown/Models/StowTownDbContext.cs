using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace StowTown.Models;

public partial class StowTownDbContext : DbContext
{
    public StowTownDbContext()
    {
    }

    public StowTownDbContext(DbContextOptions<StowTownDbContext> options)
        : base(options)
    {

    }

    public virtual DbSet<ArtistGroup> ArtistGroups { get; set; }

    public virtual DbSet<ArtistGroupMember> ArtistGroupMembers { get; set; }

    public virtual DbSet<CallRecord> CallRecords { get; set; }

    public virtual DbSet<Dj> Djs { get; set; }

    public virtual DbSet<MonthlySongList> MonthlySongLists { get; set; }

    public virtual DbSet<ProjectProducer> ProjectProducers { get; set; }

    public virtual DbSet<ProjectProducerArtistGroup> ProjectProducerArtistGroups { get; set; }

    public virtual DbSet<RadioStation> RadioStations { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<SongPossition> SongPossitions { get; set; }

    public virtual DbSet<Timing> Timings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WeekRadioTime> WeekRadioTimes { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-RJ\\SQLEXPRESS02;Initial Catalog=StowTown;Trusted_Connection=True; Encrypt=False");


    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //      => optionsBuilder.UseSqlServer("Data Source=103.195.4.8;Initial Catalog=stowtown; User Id=stowtown; Password=esh@len$1; Encrypt=False;TrustServerCertificate=True;");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("StowTownConnectionString");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArtistGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Artist");
        });

        modelBuilder.Entity<ArtistGroupMember>(entity =>
        {
            entity.HasOne(d => d.FkArtistGroupNavigation).WithMany(p => p.ArtistGroupMembers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ArtistGroupMember_ArtistGroup");
        });

        modelBuilder.Entity<CallRecord>(entity =>
        {
            entity.HasOne(d => d.FkDjNavigation).WithMany(p => p.CallRecords).HasConstraintName("FK_CallRecord_DJ");

            entity.HasOne(d => d.FkRadioStationNavigation).WithMany(p => p.CallRecords).HasConstraintName("FK_CallRecord_RadioStation");
        });

        modelBuilder.Entity<Dj>(entity =>
        {
            entity.HasOne(d => d.FkRadioStationNavigation).WithMany(p => p.Djs)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DJ_RadioStation");
        });

        modelBuilder.Entity<MonthlySongList>(entity =>
        {
            entity.HasOne(d => d.FkArtistNavigation).WithMany(p => p.MonthlySongLists).HasConstraintName("FK_MonthlySongList_ArtistGroup1");

            entity.HasOne(d => d.FkSongNavigation).WithMany(p => p.MonthlySongLists).HasConstraintName("FK_MonthlySongList_Songs");
        });

        modelBuilder.Entity<ProjectProducerArtistGroup>(entity =>
        {
            entity.HasOne(d => d.FkArtistNavigation).WithMany(p => p.ProjectProducerArtistGroups).HasConstraintName("FK_ProjectProducerArtistGroup_ArtistGroup");

            entity.HasOne(d => d.FkProjectProducerNavigation).WithMany(p => p.ProjectProducerArtistGroups).HasConstraintName("FK_ProjectProducerArtistGroup_ProjectProducer");
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasOne(d => d.FkArtistNavigation).WithMany(p => p.Songs).HasConstraintName("FK_Songs_ArtistGroup");
        });

        modelBuilder.Entity<SongPossition>(entity =>
        {
            entity.HasOne(d => d.FkMonthlySongListNavigation).WithMany(p => p.SongPossitions).HasConstraintName("FK_SongPossition_MonthlySongList");

            entity.HasOne(d => d.FkRadioStationNavigation).WithMany(p => p.SongPossitions).HasConstraintName("FK_SongPossition_RadioStation1");

            entity.HasOne(d => d.FkSongNavigation).WithMany(p => p.SongPossitions).HasConstraintName("FK_SongPossition_Songs");
        });

        modelBuilder.Entity<WeekRadioTime>(entity =>
        {
            entity.HasOne(d => d.FkDjNavigation).WithMany(p => p.WeekRadioTimes).HasConstraintName("FK_WeekRadioTime_DJ");

            entity.HasOne(d => d.FkRadioNavigation).WithMany(p => p.WeekRadioTimes).HasConstraintName("FK_WeekRadioTime_RadioStation");

            entity.HasOne(d => d.FkTimingNavigation).WithMany(p => p.WeekRadioTimes).HasConstraintName("FK_WeekRadioTime_Timings");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
