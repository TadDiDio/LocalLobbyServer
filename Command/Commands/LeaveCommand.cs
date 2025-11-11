using LobbyService.LocalServer;

public class LeaveCommand : BaseCommand<LeaveLobbyRequest>
{
    public override Message Execute(CommandContext ctx, LeaveLobbyRequest request)
    {
        ctx.LobbyManager.LeaveLobby(request, ctx.Sender);
        return Message.CreateResponse(new DummyResponse(), ctx.RequestId);
    }
}