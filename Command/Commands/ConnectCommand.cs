namespace LobbyService.LocalServer;

public class ConnectCommand : BaseCommand<ConnectRequest>
{
    public override Message Execute(CommandContext ctx, ConnectRequest request)
    {
        return Message.CreateResponse(new WelcomeResponse { LocalMember = ctx.Sender }, ctx.RequestId);
    }
}