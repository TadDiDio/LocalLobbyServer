using System;

namespace LobbyService.LocalServer;

public class ChatMessageCommand : BaseCommand<ChatMessageRequest>
{
    public override Message Execute(CommandContext ctx, ChatMessageRequest request)
    {
        ctx.LobbyManager.SendChat(request, ctx.Sender);
        return Message.CreateResponse(new NullResponse(), ctx.RequestId);
    }
}