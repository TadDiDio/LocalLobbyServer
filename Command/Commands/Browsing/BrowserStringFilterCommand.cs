namespace LobbyService.LocalServer;

public class BrowserStringFilterCommand : BaseCommand<ApplyStringFilterRequest>
{
    public override Message Execute(CommandContext ctx, ApplyStringFilterRequest request)
    {
        ctx.LobbyManager.Browser.ApplyStringFilter(request);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}