namespace LobbyService.LocalServer;

public class JoinCommand : BaseCommand<JoinLobbyRequest>
{
    public override Message Execute(CommandContext ctx, JoinLobbyRequest request)
    {
        var lobby = ctx.LobbyManager.JoinLobby(request, ctx.Sender);

        if (lobby == null) return Message.CreateFailure(Error.InvalidId, ctx.RequestId);

        return Message.CreateResponse(new EnterResponse
        {
            Snapshot = lobby.GetSnapshot()
        }, ctx.RequestId);
    }
}