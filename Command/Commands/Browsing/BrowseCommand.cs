namespace LobbyService.LocalServer;

public class BrowseCommand : BaseCommand<BrowseRequest>
{
    public override Message Execute(CommandContext ctx, BrowseRequest request)
    {
        var lobbies = ctx.LobbyManager.Browser.GetLobbies();
        return Message.CreateResponse(new BrowseResponse
        {
            Snapshots = lobbies
        }, ctx.RequestId);
    }
}