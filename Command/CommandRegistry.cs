using System;
using System.Collections.Generic;
using LobbyService.LocalServer;

public static class CommandRegistry
{
    private static Dictionary<Type, Type> _types;

    static CommandRegistry()
    {
        _types = new Dictionary<Type, Type>
        {
            [typeof(CreateLobbyRequest)] = typeof(CreateCommand),

        };
    }

    public static bool TryGet(IRequest request, out ICommand command)
    {
        if (!_types.TryGetValue(request.GetType(), out var cmdType))
        {
            command = null;
            return false;
        }

        var instance = (ICommand)Activator.CreateInstance(cmdType);
        instance.SetRequest(request);
        command = instance;
        return true;
    }
}