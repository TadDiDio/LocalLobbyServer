using System;

namespace LobbyService.LocalServer;

public class KickCommand : BaseCommand<KickMemberRequest>
{
    public override Message Execute(CommandContext ctx, KickMemberRequest request)
    {
        ctx.LobbyManager.Kick(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}