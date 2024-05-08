using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Holiday.Api.Core.Configurations;
using Holiday.Api.Repository.Models;
using Microsoft.IdentityModel.Tokens;

namespace Holiday.Api.Core.Utils.Jwt;

public class JwtUtils
{
    /// <summary>
    /// Génère un jeton JWT (JSON Web Token) en utilisant la configuration JWT fournie et les informations de l'utilisateur.
    /// </summary>
    /// <param name="jwtConfiguration">La configuration JWT contenant la clé secrète, l'émetteur, et l'audience.</param>
    /// <param name="user">Les informations de l'utilisateur pour lesquelles le jeton est généré.</param>
    /// <returns>Le jeton JWT généré.</returns>
    public static string GenerateJwtToken(JwtConfiguration jwtConfiguration, Participant user)
    {

        var jwtTokenHandler = new JwtSecurityTokenHandler();
        
        var secretKeyInBytes = Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            // Ecrire les données contenues dans le payload
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                // identifiant unique qui va être spécifique pour le jeton et l'utilisateur
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())

            }),
            // après combien de temps le jeton va experirer
            Expires = DateTime.Now.AddHours(3),
            Issuer = jwtConfiguration.Issuer,
            Audience = jwtConfiguration.Audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secretKeyInBytes), SecurityAlgorithms.HmacSha256)

        };
        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Cette méthode permet de vérifier si le tokenId Google renvoyé par un des clients applicatifs
    /// est valide. Pour cela, on doit préciser le clientId que notre API partage avec google afin de vérifier
    /// l'audience, tous les autres vérificants sont automatiquements effectuées par la méthode ValidateAsync comme
    /// le tokenId null, vide, mauvaise algorithme, mauvaise signature et l'invaladité du token (temps).
    /// </summary>
    /// <param name="tokenId">Le JWT renvoyé par google pour un client qui tente de se connecter</param>
    /// <param name="clientId">Le clientId que l'on partage avec Google</param>
    /// <returns>le contenu du payload si le JWT est valide, sinon null</returns>
    public static async Task<GoogleJsonWebSignature.Payload?> VerifiyGoogleToken(string tokenId, string clientId)
    {
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var paylaod = await GoogleJsonWebSignature.ValidateAsync(tokenId, validationSettings);
            return paylaod;
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
