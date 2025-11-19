namespace LobbyService.LocalServer;

public class BrowserNumberFilterCommand : BaseCommand<ApplyNumberFilterRequest>
{
    public override Message Execute(CommandContext ctx, ApplyNumberFilterRequest request)
    {
        ctx.LobbyManager.Browser.ApplyNumberFilter(request);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}