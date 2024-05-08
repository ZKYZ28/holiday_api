using AutoMapper;
using Holiday.Api.Contract.Dto;
using Holiday.Api.Core.Configurations;
using Holiday.Api.Core.Utilities;
using Holiday.Api.Core.Utils.Jwt;
using Holiday.Api.Repository.CustomErrors;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Holiday.Api.Core.Controllers;

[AllowAnonymous]
[ApiController]
[Route("/v1/authentification")]
public class AuthentificationController : ControllerBase
{

    private readonly UserManager<Participant> _userManager;
    private readonly JwtConfiguration _jwtConfiguration;
    private readonly GoogleConfiguration _googleConfiguration;
    private readonly IAuthRepository _authRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthentificationController> _logger;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe AuthentificationController.
    /// </summary>
    /// <param name="logger">Journalisation utilisé pour enregistrer des messages et des informations de journalisation.</param>
    /// <param name="userManager">Le gestionnaire d'utilisateurs permettant de gérer les participants.</param>
    /// <param name="jwtConfiguration">La configuration JWT pour la gestion des jetons d'authentification.</param>
    /// <param name="googleConfiguration">Interface qui permet de récupérer des inforamtions de configuration</param>
    /// <param name="mapper">L'objet de mappage utilisé pour mapper des objets entre différents modèles.</param>
    /// <param name="authRepository">Interface qui permet l'accès aux données d'authentification.</param>
    public AuthentificationController(
        ILogger<AuthentificationController> logger,
        [FromServices] UserManager<Participant> userManager, 
        [FromServices] IOptions<JwtConfiguration> jwtConfiguration,
        [FromServices] IOptions<GoogleConfiguration> googleConfiguration,
        [FromServices] IMapper mapper,  
        [FromServices] IAuthRepository authRepository
        )
    {
        _userManager = userManager;
        _jwtConfiguration = jwtConfiguration.Value;
        _googleConfiguration = googleConfiguration.Value;
        _mapper = mapper;
        _authRepository = authRepository;
        _logger = logger;
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur avec les données fournies dans le corps de la requête.
    /// </summary>
    /// <param name="newUser">Les données du nouvel utilisateur à inscrire.</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec le jeton JWT si l'inscription réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur si l'adresse e-mail est déjà utilisée ou si une erreur survient lors de l'inscription.
    /// </returns>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] NewParticipantDto newUser)
    {
        // Check if the user is already exists
        var userExists = await _userManager.FindByEmailAsync(newUser.Email);
        if (userExists != null)
        {
            _logger.LogError("L'adresse email fournie par l'utilisateur est déjà utilisée.");
            return BadRequest("L'adresse mail est déjà utilisé. Merci de vous connecter.");
        }

        var participantEntity = _mapper.Map<Participant>(newUser);
        participantEntity.UserName = participantEntity.Email;

        var isCreated = await _userManager.CreateAsync(participantEntity, newUser.Password);

        if (isCreated.Succeeded)
        {
                    
            if (!EmailSender.SendEmailFromApi(participantEntity.Email, $"Bonjour,\n\nNous vous confirmons la création de votre compte avec l'addresse mail : {participantEntity.Email} sur notre plateforme !\n\nPassez un bon moment sur Holiday ! ", false))
            {
                _logger.LogError("Une erreur est survenue durant l'envoie du mail de confirmation lors d'une inscription.");
            }
            // Générer le token + si utilisateur est bien créé, on a directement l'id dans l'entité via la méthode CreateAsync. C'est "important" pour la génération du token
            var token = JwtUtils.GenerateJwtToken(_jwtConfiguration, participantEntity);

            _logger.LogInformation("L'utilisateur ayant l'adresse {EmailAddress} a été enregistré avec succès.", participantEntity.Email);
            return Ok(token);

        }
        
        _logger.LogError("Une erreur est survenue lors de l'inscription de l'utilisateur.");
        return BadRequest("Erreur lors de l'inscription de l'utilisateur");
    }

    /// <summary>
    /// Authentifie un utilisateur en vérifiant les informations de connexion fournies dans le corps de la requête.
    /// </summary>
    /// <param name="loginDto">Les données de connexion de l'utilisateur (adresse e-mail et mot de passe).</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec le jeton JWT si l'authentification réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Aucun compte n'est associé à l'adresse e-mail fournie.
    ///   - Les informations d'identification sont incorrectes.
    ///   - Une exception est levée lors de la récupération des informations de l'utilisateur.
    /// </returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] AccountLoginDto loginDto)
    {
        // Check if the user exists
        var userExists = await _userManager.FindByEmailAsync(loginDto.Email);
        if (userExists == null)
        {
            _logger.LogError("Aucune compte n'est lié à l'adresse {EmailAddress}.", loginDto.Email);
            return BadRequest("Aucun compte n'est lié à cette adresse mail.");
        }

        if (!await _userManager.CheckPasswordAsync(userExists, loginDto.Password))
        {
            _logger.LogError("Les informations de connexion fournies ne sont pas valides.");
            return BadRequest("Les informations de connexion fournies ne sont pas valides.");
        }

        Participant participant = null;
        try
        {
             participant = await _authRepository.GetUserByEmail(loginDto.Email);
        }
        catch (LoadDataBaseException e)
        {
            _logger.LogError("Une erreur s'est produite lors de la récupération de l'utilisateur {EmailAddress}.", loginDto.Email);
            return BadRequest(e.Message);
        }

        // Retrieve information about the user
        var token = JwtUtils.GenerateJwtToken(_jwtConfiguration, participant);

        _logger.LogInformation("Connexion effectuée au compte {EmailAddress}.", loginDto.Email);
        return Ok(token);
    }

    /// <summary>
    /// Permet de se connecter avec un login externe, google.
    /// </summary>
    /// <param name="googleLoginDto">TokenId (JWT Google) envoyé par un client applicatif</param>
    /// <returns>
    /// - StatusCode 200 (OK) avec le jeton JWT si l'authentification réussit.
    /// - StatusCode 400 (BadRequest) avec un message d'erreur dans les cas suivants :
    ///   - Le JWT Google (TokenId), envoyé par un client applicatif n'est pas lié à notre API.
    /// </returns>
    [HttpPost]
    [Route("googleauth")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleLoginDto googleLoginDto)
    {
        
            var paylaod = await JwtUtils.VerifiyGoogleToken(googleLoginDto.tokenId, _googleConfiguration.ClientId);
            if (paylaod == null)
                return BadRequest("Une erreur dans l'authentification Google est survenue, veuillez réessayer !");
            
            // le subject du payload est l'id permanent de l'utilisateur chez Google
            var info = new UserLoginInfo(ExternalServicesConfiguration.GoogleSubSection, paylaod.Subject,
                ExternalServicesConfiguration.GoogleSubSection);

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                // Vérifier si l'utilisateur a déjà un compte normal, si c'est le cas. On va ajouter
                // qu'il peut également se connecter maintenant se connecter via google
                user = await _userManager.FindByEmailAsync(paylaod.Email);

                if (user == null)
                {
                    user = Participant.CreateExternalLoginParticipant(paylaod.Email, paylaod.FamilyName, paylaod.GivenName);
                    // Créer un user, sans mot de passe
                    await _userManager.CreateAsync(user);
                    if (!EmailSender.SendEmailFromApi(paylaod.Email, $"Bonjour,\n\nMerci {paylaod.GivenName} {paylaod.FamilyName} d'utiliser notre plateforme et notre service externe Google !\n\nPassez un bon moment sur Holiday ! ", false))
                    {
                        _logger.LogError("Une erreur est survenue durant l'envoie du mail de confirmation lors d'une inscription.");
                    }
                    // Cette méthode va permettre de retenir dans la table 'AspNetUserLogins' qu'un utilisateur 
                    // s'est connecté avec un login externe, on pourra récupérer ses informations plus vite, la prochaine
                    // fois
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    _logger.LogError("Tentative de connexion avec Google avec une adresse mail déjà utilisé dans l'authentification simple {EmailAdress}", paylaod.Email);
                    return BadRequest("L'adresse mail est déjà utilisé.");
                }

            }
            else
            {
                // User déjà connecté par google, on va s'assurer que ces informations soient toujours à jour
                // pour le nom et prénom. Cependant, une adresse mail est inmodifiable
                user.FirstName = paylaod.GivenName;
                if (paylaod.FamilyName != null)
                {
                    user.LastName = paylaod.FamilyName;
                }

                await _userManager.UpdateAsync(user);
            }
            // Generate JWT for the user
            string token = JwtUtils.GenerateJwtToken(_jwtConfiguration, user);
            
            _logger.LogInformation("Connexion avec Google effectuée.");
            return Ok(token);
        }
}