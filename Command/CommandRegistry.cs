using System;
using System.Collections.Generic;

namespace LobbyService.LocalServer;

public static class CommandRegistry
{
    private static Dictionary<Type, Type> _types;

    static CommandRegistry()
    {
        _types = new Dictionary<Type, Type>
        {
            [typeof(ConnectRequest)] = typeof(ConnectCommand),
            [typeof(CreateLobbyRequest)] = typeof(CreateCommand),
            [typeof(JoinLobbyRequest)] = typeof(JoinCommand),
            [typeof(LeaveLobbyRequest)] = typeof(LeaveCommand),
            [typeof(InviteMemberRequest)] = typeof(InviteCommand),

            [typeof(QueryFriendsRequest)] = typeof(QueryFriendsCommand)
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