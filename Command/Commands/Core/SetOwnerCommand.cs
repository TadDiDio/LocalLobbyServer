namespace LobbyService.LocalServer;

public class SetOwnerCommand : BaseCommand<SetOwnerRequest>
{
    public override Message Execute(CommandContext ctx, SetOwnerRequest request)
    {
        ctx.LobbyManager.SetOwner(request.LobbyId, request.NewOwnerId);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}