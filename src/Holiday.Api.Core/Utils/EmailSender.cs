using System.Net.Mail;

namespace Holiday.Api.Core.Utilities;

public static class EmailSender
{
    private const string SmtpServer = "smtp.helmo.be"; 
    private const int SmtpPort = 25;
    private const string Username = "no-reply@holiday.com";
    private const string ToAddress = "f.mahy@student.helmo.be";
    private const string AdminSubject = "Requête pour Holiday.";
    private const string ConfirmationSubject = "Confirmation de création de compte";
    
    
    /// <summary>
    /// La méthode SendEmailFromApi est utilisée pour envoyer des emails via un serveur SMTP.
    /// Elle permet de spécifier si l'adresse email fournie doit être utilisée comme expéditeur ou destinataire de l'email.
    /// </summary>
    /// <param name="mailAddress">L'adresse email qui sera utilisée soit comme expéditeur, soit comme destinataire de l'email, selon la valeur du paramètre isSender</param>
    /// <param name="content">Le contenu de l'email à envoyer.</param>
    /// <param name="isSender"> Un booléen qui détermine comment l'adresse email fournie en paramètre sera utilisée.
    /// Si true, mailAddress est utilisée comme expéditeur, et username (constante de classe) comme destinataire.
    /// Si false, mailAddress est utilisée comme destinataire, et username comme expéditeur.</param>
    /// <returns>true si l'email est envoyé avec succès sinon false</returns>
    public static bool SendEmailFromApi (string mailAddress, string content, bool isSender)
    {
        
        try
        {
            using (SmtpClient smtpClient = new SmtpClient(SmtpServer, SmtpPort))
            {

                MailMessage mail;
                if (isSender)
                {
                    mail = new MailMessage(mailAddress, ToAddress, AdminSubject, content);
                }
                else
                {
                    mail = new MailMessage(Username, mailAddress, ConfirmationSubject, content);
                }
                
                smtpClient.Send(mail);
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }
}