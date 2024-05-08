using Holiday.Api.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Holiday.Api.Repository.EntityConfigurations;

public class HolidayEntityTypeConfiguration  : IEntityTypeConfiguration<Models.Holiday>
{
    public void Configure(EntityTypeBuilder<Models.Holiday> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Location)
            .WithOne()
            .HasForeignKey<Models.Holiday>(x=> x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}