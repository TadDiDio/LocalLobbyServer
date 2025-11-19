using System.Linq;

namespace LobbyService.LocalServer;

public class QueryFriendsCommand : BaseCommand<QueryFriendsRequest>
{
    public override Message Execute(CommandContext ctx, QueryFriendsRequest request)
    {
        return Message.CreateResponse(new QueryFriendsResponse
        {
            Friends = [.. ctx.LobbyManager.ConnectedMembers.Where(m => m != ctx.Sender)]
        }, ctx.RequestId);
    }
}