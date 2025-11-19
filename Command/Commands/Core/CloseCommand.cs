namespace LobbyService.LocalServer;

public class CloseCommand : BaseCommand<CloseLobbyRequest>
{
    public override Message Execute(CommandContext ctx, CloseLobbyRequest request)
    {
        ctx.LobbyManager.CloseLobby(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}