namespace LobbyService.LocalServer;

public class DirectMessageCommand : BaseCommand<DirectMessageRequest>
{
    public override Message Execute(CommandContext ctx, DirectMessageRequest request)
    {
        ctx.LobbyManager.SendDirectChat(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}