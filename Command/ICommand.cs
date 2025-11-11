using System;

namespace LobbyService.LocalServer;

public interface ICommand
{
    public void SetRequest(IRequest request);
    public Message Execute(CommandContext ctx);
}

public abstract class BaseCommand<TRequest> : ICommand where TRequest : IRequest
{
    private TRequest _request;
    public void SetRequest(IRequest request)
    {
        if (request is not TRequest typed) throw new ArgumentException("Ensure the request type matches the command type.");

        _request = typed;
    }

    public Message Execute(CommandContext ctx) => Execute(ctx, _request);
    public abstract Message Execute(CommandContext ctx, TRequest request);
}