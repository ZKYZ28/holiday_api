using Holiday.Api.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Holiday.Api.Repository;

public static class PersistanceServiceExtension
{
    public static void AddPersistanceServices(this IServiceCollection services)
    {
        // Add repo
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IParticipateRepository, ParticipateRepository>();
        
    }
}