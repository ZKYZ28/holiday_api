using Holiday.Api.Core.Extensions;
using Holiday.Api.Core.Helpers;
using Holiday.Api.Core.Hubs;
using Holiday.Api.Repository;
using Holiday.Api.Repository.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
BuilderHelper.ConfigureBuilder(builder);


var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Participant>>();
    var context= scope.ServiceProvider.GetRequiredService<HolidayDbContext>();
}

// Serveur wwwroot pour le stockage d'image : https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-7.0
app.UseStaticFiles();

app.UseHttpsRedirection();

// Authentification
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.UseCors();

//SIGNAL R MAP
app.MapHub<ChatHub>("/chat");

app.Run();

public partial class Program {}

