namespace LobbyService.LocalServer;

public class SetMemberDataCommand : BaseCommand<MemberDataRequest>
{
    public override Message Execute(CommandContext ctx, MemberDataRequest request)
    {
        ctx.LobbyManager.SetMemberData(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}