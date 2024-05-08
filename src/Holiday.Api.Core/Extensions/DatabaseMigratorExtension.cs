using Holiday.Api.Repository;
using Microsoft.EntityFrameworkCore;

namespace Holiday.Api.Core.Extensions;

public static class DatabaseMigratorExtension
{
    public static void UseDatabaseMigrator(this WebApplication app)
    {
        var localScope = app.Services.CreateScope();
        var dbContext = localScope.ServiceProvider.GetService<HolidayDbContext>();
        dbContext?.Database.Migrate();
    }
}