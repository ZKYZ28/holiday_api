using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Holiday.Api.Repository.EntityConfigurations;

public class ChatEntityTypeConfiguration  : IEntityTypeConfiguration<Models.Holiday>
{
    public void Configure(EntityTypeBuilder<Models.Holiday> builder)
    {
        builder.HasKey(x => x.Id);
    }
}