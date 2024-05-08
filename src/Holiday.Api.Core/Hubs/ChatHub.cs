using Holiday.Api.Contract.Dto;
using Holiday.Api.Repository.Models;
using Holiday.Api.Repository.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Holiday.Api.Core.Hubs;

public class ChatHub : Hub
{
    private readonly string _bothUserFirstName;
    private readonly string _bothUserLastName;
    private readonly IMessageRepository _messageRepository;
    
    /// <summary>
    /// Initialise une nouvelle instance de la classe ChatHub.
    /// </summary>
    /// <param name="messageRepository">Interface qui permet l'accès aux données des messages des différents chats.</param>
    public ChatHub([FromServices] IMessageRepository messageRepository)
    {
        _bothUserFirstName = "Cat";
        _bothUserLastName = "Chat";
        _messageRepository = messageRepository;
    }
    
    /// <summary>
    /// Méthode permettant à un utilisateur de rejoindre une salle de discussion pour une vacances.
    /// </summary>
    /// <param name="holidayId">L'id de la vacances qu'il veut rejoindre</param>
    /// <param name="user">L'utilisateur connecté qui souhaite rejoindre</param>
    /// <returns>Un JSON avec un message de bienvenue</returns>
    public async Task JoinRoom(string holidayId, UserAuthentificatedDto user)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, holidayId);
        
        var messageToSend = new Message
        {
            SendAt = DateTimeOffset.Now,
            Content = $"{user.FirstName} a rejoint le groupe.",
            ParticipantId = user.Id,
            Participant = new Participant{
                FirstName = _bothUserFirstName,
                LastName = _bothUserLastName
            }
        };
        
        await Clients.Client(Context.ConnectionId).SendAsync("ClearMessages");
        await SendMessageHistory(holidayId);
        await Clients.Group(holidayId).SendAsync("ReceiveSendMessage", messageToSend);
    }
    
    /// <summary>
    /// Méthode permettant à un utilisateur de quitter une salle de discussion pour une vacances.
    /// </summary>
    /// <param name="holidayId">L'id de la vacances qu'il veut quitter.</param>
    /// <param name="user">L'utilisateur connecté qui souhaite quitter</param>
    /// <returns>Un JSON avec un message de indiquant le départ</returns>
    public async Task LeaveHolidayRoom(string holidayId, UserAuthentificatedDto user)
    {
        var messageToSend = new Message
        {
            SendAt = DateTimeOffset.Now,
            Content = $"{user.FirstName} a quitté le groupe.",
            ParticipantId = user.Id,
            Participant = new Participant{
                FirstName = _bothUserFirstName,
                LastName = _bothUserLastName
            }
        };
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, holidayId);
        await Clients.Group(holidayId).SendAsync("ReceiveSendMessage", messageToSend);
    }

    /// <summary>
    /// Méthode permettant à un utilisateur d'envoyer un message dans une salle de discussion pour une vacances.
    /// Stocke également le message dans la base de données
    /// </summary>
    /// <param name="user">L'utilisateur connecté qui envoie le message.</param>
    /// <param name="holidayId">L'id de la vacances qui représente le groupe dans lequel il veut envoyer son message</param>
    /// <param name="message">Contenu du message à envoyer.</param>
    /// <returns>Un JSON contenant son message</returns>
    public async Task SendMessage(UserAuthentificatedDto user, string holidayId, string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            
            var messageToSend = new Message
            {
                SendAt = DateTimeOffset.Now,
                Content = message,
                ParticipantId = user.Id,
                Participant = new Participant{
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
            
            await Clients.Group(holidayId).SendAsync("ReceiveSendMessage", messageToSend);
            await _messageRepository.AddMessage(user.Id, new Guid(holidayId), message);
        }
    }

    /// <summary>
    /// Méthode permettant d'envoyer l'historique des 100 derniers messages à un utilisateur spécifique dans une salle de discussion pour une vacances.
    /// </summary>
    /// <param name="holidayId">L'id de la vacances</param>
    /// <returns>Un JSON avec au maximum les 100 derniers messages.</returns>
    private async Task SendMessageHistory(string holidayId)
    {
        var listMessageHistory = await _messageRepository.GetAllMessageByHoliday(new Guid(holidayId));

        if (listMessageHistory.Count == 0)
        {
            return;
        }
        
        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveHistoryMessage", listMessageHistory);
    }
}