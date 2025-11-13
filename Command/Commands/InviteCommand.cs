using System;

namespace LobbyService.LocalServer;

public class InviteCommand : BaseCommand<InviteMemberRequest>
{
    public override Message Execute(CommandContext ctx, InviteMemberRequest request)
    {
        ctx.LobbyManager.Invite(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}