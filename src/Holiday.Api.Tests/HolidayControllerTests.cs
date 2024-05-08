using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using DefaultNamespace;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository;
using Holiday.Api.Repository.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;

namespace Tests;

public class HolidayControllerTests
{
    private HttpClient _client;
    private HolidayDbContext _context;

    public HolidayControllerTests()
    {
        // Configuration de la base de données en mémoire
        var appFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remplacer la configuration de la base de données par une base de données en mémoire
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == 
                             typeof(DbContextOptions<HolidayDbContext>));

                    // Suppression de la configuration de la base de données de développement
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    

                    services.AddDbContext<HolidayDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });

        _client = appFactory.CreateClient();
        // Simuler la manière dont ASP.NET core traite les requêtes dans un envrionnement réel.
        // C'est-à-dire, où chaque requête est traitée dans son propre scope.
        var scope = appFactory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<HolidayDbContext>();
    }
    
    [SetUp]
    public void Setup()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Test]
    public async Task TestRegister()
    {
        // 1. Inscription de l'utilisateur
        var newUser = createAParticipant("Jean", "Dupont", "jean.dupont@gmail.com", "Passw0rd!");
        var registerResponse = await _client.PostAsJsonAsync("/v1/authentification/register", newUser);
        Assert.AreEqual(HttpStatusCode.OK, registerResponse.StatusCode);
        Assert.IsTrue(registerResponse.IsSuccessStatusCode);
        Assert.IsNotEmpty(await registerResponse.Content.ReadAsStringAsync());
        Assert.IsNotNull(await registerResponse.Content.ReadAsStringAsync());
    }
    
    [Test]
    public async Task TestLoginWithoutRegister()
    {
        // 1. Inscription de l'utilisateur
        var loginUser =  new AccountLoginDto { Email = "john.doe@gmail.com", Password = "Passw0rd!" };
        var loginResponse  = await _client.PostAsJsonAsync("/v1/authentification/login", loginUser);
        Assert.AreEqual(HttpStatusCode.BadRequest, loginResponse.StatusCode);
        Assert.False(loginResponse.IsSuccessStatusCode);
        var token = await loginResponse.Content.ReadAsStringAsync();
        Console.WriteLine(token);
    }
    
    [Test]
    public async Task TestCreateHolidayAndGetIt()
    {
        try
        {
            var newUser = createAParticipant("John", "Doe", "john.doe@gmail.com", "Passw0rd!");
            var registerResponse = await _client.PostAsJsonAsync("/v1/authentification/register", newUser);
            Assert.AreEqual(HttpStatusCode.OK, registerResponse.StatusCode);

            // 1. Login de l'utilisateur créé précédémment avec l'inscription
            var loginDto = new AccountLoginDto { Email = "john.doe@gmail.com", Password = "Passw0rd!" };

            var loginResponse = await _client.PostAsJsonAsync("/v1/authentification/login", loginDto);
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.IsNotEmpty(await loginResponse.Content.ReadAsStringAsync());
            var token = await loginResponse.Content.ReadAsStringAsync();

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken?.Claims.First(claim => claim.Type == "nameid").Value;
            Assert.IsNotNull(userId);
            Assert.IsNotEmpty(userId);


            var locationDto = CreateLocationDto("Liège", "Belgique", "4000");
            // 2. Création de la holiday avec utilisateur authentifié
            var holidayDto = CreateHolidayDto("Monaco 2023-2024", "On va s'amusrer", userId, DateTimeOffset.Now,
                DateTimeOffset.Now.AddDays(7), locationDto);

            var formData = CreateHolidayFormData(holidayDto);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var createHolidayResponse = await _client.PostAsync("/v1/holidays/", formData);
            Assert.AreEqual(HttpStatusCode.OK, createHolidayResponse.StatusCode);

            // Récupérer la liste de holiday du participant pour voir si ça lui a bien été ajouté
            var getAllHolidaysForTheParticipantResponse = await _client.GetAsync($"/v1/holidays/participant/{userId}");
            Assert.AreEqual(HttpStatusCode.OK, getAllHolidaysForTheParticipantResponse.StatusCode);

            var holidaysReponseContent = await getAllHolidaysForTheParticipantResponse.Content.ReadAsStringAsync();
            var holidays = JsonConvert.DeserializeObject<ICollection<HolidayOutDto>>(holidaysReponseContent);

            // Vérifier si la liste des vacances obtenues contient au moins une vacance
            Assert.IsNotNull(holidays);
            Assert.IsTrue(holidays.Count > 0);
        }
        catch (JsonSerializationException)
        {
            Assert.Fail("Une erreur est survenue lors de la désérialisation du séjour.");
        }
        catch (ArgumentException)
        {
            Assert.Fail("Une erreur est survenue lors de la récupération du token pour l'utilisateur authentifié");
        }
        catch (Exception e)
        {
            Assert.Fail($"Une erreur innatendue est survenue dans le test TestCreateHolidayAndGetIt : ${e.Message}");
        }
    }

    private static NewParticipantDto createAParticipant(string firstName, string lastName, string email, string password)
    {
        return new NewParticipantDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password
        };
    }
    
    private static MultipartFormDataContent CreateHolidayFormData(HolidayInDto holidayDto)
    {
        var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(holidayDto.Name), "name");
        formData.Add(new StringContent(holidayDto.Description), "description");
        formData.Add(new StringContent(holidayDto.CreatorId), "creatorId");
        formData.Add(new StringContent(holidayDto.StartDate.ToString("o")), "startDate"); // Format ISO 8601 (le paramètre "o", on conserve le même fomat qu'en flutter")
        formData.Add(new StringContent(holidayDto.EndDate.ToString("o")), "endDate");
        formData.Add(new StringContent(holidayDto.Location.Country), "location.Country");
        formData.Add(new StringContent(holidayDto.Location.Locality), "location.Locality");
        formData.Add(new StringContent(holidayDto.Location.PostalCode), "location.PostalCode");

        return formData;
    }
    
    public HolidayInDto CreateHolidayDto(string name, string description, string creatorId, DateTimeOffset startDate, DateTimeOffset endDate, LocationDto location)
    {
        return new HolidayInDto
        {
            Name = name,
            Description = description,
            CreatorId = creatorId,
            StartDate = startDate,
            EndDate = endDate,
            Location = location
        };
    }
    
    public LocationDto CreateLocationDto(string country, string locality, string postalCode, string? number = null, string? street = null)
    {
        return new LocationDto
        {
            Country = country,
            Locality = locality,
            PostalCode = postalCode,
            Number = number,
            Street = street,
        };
    }


}