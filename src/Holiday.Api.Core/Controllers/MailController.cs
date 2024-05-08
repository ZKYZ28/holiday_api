using Holiday.Api.Contract.Dto;
using Holiday.Api.Core.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Holiday.Api.Core.Controllers;

[ApiController]
[Route("/v1/mail")]
public class MailController : ControllerBase
{
    private readonly ILogger<MailController> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de la classe MailController.
    /// </summary>
    /// <param name="logger">Un service de journalisation permettant de créer des journaux pour cette classe.</param>
    public MailController(ILogger<MailController> logger)
    {
        this._logger = logger;
    }
    
    
    /// <summary>
    /// Envoie un e-mail en utilisant les données fournies dans un objet MailDto.
    /// </summary>
    /// <param name="mail">Un objet MailDto contenant les informations nécessaires pour l'e-mail.</param>
    /// <returns>Un résultat HTTP indiquant le succès ou l'échec de l'envoi de l'e-mail.</returns>
    [HttpPost]
    public async Task<IActionResult> SendMail([FromBody] MailDto mail)
    {
        if (string.IsNullOrWhiteSpace(mail.Content) || string.IsNullOrEmpty(mail.Content) || mail.Content.Length < 5)
        {
            _logger.LogError("Un mail a envoyé à l'administrateur n'est pas valide.");
            return BadRequest("Informations non valides.");
        }

        if (!EmailSender.SendEmailFromApi(mail.SenderEmail, mail.Content, true))
        {
            _logger.LogError("Erreur lors de l'envoie d'un mail à l'administrateur.");
            return BadRequest("Erreur lors de l'envoie du mail.");
        }
        
        _logger.LogInformation("Mail envoyé avec succès à l'administrateur.");
        return Ok("Mail envoyé avec succès.");
    }
}