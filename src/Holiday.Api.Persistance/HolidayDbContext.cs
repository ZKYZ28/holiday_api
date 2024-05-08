using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Models.enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Repository;

// To run in the Core project folder 'Holiday.Api.Core'
// dotnet ef migrations add "First" --project ..\Holiday.Api.Persistance\Holiday.APi.Persistance.csproj
public class HolidayDbContext : IdentityDbContext
{
    public HolidayDbContext(DbContextOptions<HolidayDbContext> options) : base(options) { }
    
    public DbSet<Participate> Participates { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Models.Holiday> Holiday { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Message> Messages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //ACTIVITY
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Location)
                .WithOne()
                .HasForeignKey<Models.Activity>(x=> x.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        
        modelBuilder.Entity<Activity>()
            .HasOne<Models.Holiday>()  
            .WithMany(h => h.Activities)  
            .HasForeignKey(a => a.HolidayId);
        

        //HOLIDAY
        modelBuilder.Entity<Models.Holiday>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Location)
                .WithOne()
                .HasForeignKey<Models.Holiday>(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //INVITATION
        modelBuilder.Entity<Invitation>()
            .HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(i => i.ParticipantId);
        
        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.Holiday)
            .WithMany()
            .HasForeignKey(i => i.HolidayId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        //PARTICIPATEACTIVITY
        modelBuilder.Entity<Participate>()
            .HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(i => i.ParticipantId);
        
        modelBuilder.Entity<Participate>()
            .HasOne(i => i.Activity)
            .WithMany()
            .HasForeignKey(i => i.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        //PARTICIPANT
        modelBuilder.Entity<Message>()
            .HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(i => i.ParticipantId);
        
        modelBuilder.Entity<Message>()
            .HasOne(i => i.Holiday)
            .WithMany()
            .HasForeignKey(i => i.HolidayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}