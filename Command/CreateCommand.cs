using System;

namespace LobbyService.LocalServer;

public class CreateCommand : BaseCommand<CreateLobbyRequest>
{
    public override Message Execute(CommandContext ctx, CreateLobbyRequest request)
    {
        // TEMP, get guid from lobby manager.
        var response = new EnterResponse
        {
            LobbyId = Guid.NewGuid().ToString()
        };
        
        return Message.CreateResponse(response, ctx.RequestId);
    }
}   