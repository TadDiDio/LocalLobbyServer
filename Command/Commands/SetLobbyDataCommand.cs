namespace LobbyService.LocalServer;

public class SetLobbyDataCommand : BaseCommand<LobbyDataRequest>
{
    public override Message Execute(CommandContext ctx, LobbyDataRequest request)
    {
        ctx.LobbyManager.SetLobbyData(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}