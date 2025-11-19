namespace LobbyService.LocalServer;

public class BrowserSlotsAvailableFilterCommand : BaseCommand<ApplySlotsAvailableFilterRequest>
{
    public override Message Execute(CommandContext ctx, ApplySlotsAvailableFilterRequest request)
    {
        ctx.LobbyManager.Browser.ApplySlotsAvailableFilter(request);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}