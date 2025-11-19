namespace LobbyService.LocalServer;

public class ChatMessageCommand : BaseCommand<ChatMessageRequest>
{
    public Message Execute(CommandContext ctx, LocalLobbyMember sender)
    {
        
    }
}