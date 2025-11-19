namespace LobbyService.LocalServer;

public class BrowserLimitResponsesFilterCommand : BaseCommand<ApplyLimitResponsesFilterRequest>
{
    public override Message Execute(CommandContext ctx, ApplyLimitResponsesFilterRequest request)
    {
        ctx.LobbyManager.Browser.ApplyLimitResponsesFilter(request);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}