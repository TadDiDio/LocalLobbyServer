using LobbyService.LocalServer;

public interface ICommand
{
    public Message Execute(CommandContext ctx);
}