namespace LobbyService.LocalServer;

public class CreateCommand : BaseCommand<CreateLobbyRequest>
{
    public override Message Execute(CommandContext ctx, CreateLobbyRequest request)
    {
        var response = ctx.LobbyManager.CreateLobby(request, ctx.Sender);
        return Message.CreateResponse(response, ctx.RequestId);
    }
}   