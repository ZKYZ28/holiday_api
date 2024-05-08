using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Holiday.Api.Repository.EntityConfigurations;

public class ActivityEntityTypeConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Models.Activity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Location)
            .WithOne()
            .HasForeignKey<Models.Activity>(x=> x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}