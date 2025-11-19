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
            // Core
            [typeof(ConnectRequest)]      = typeof(ConnectCommand),
            [typeof(CreateLobbyRequest)]  = typeof(CreateCommand),
            [typeof(JoinLobbyRequest)]    = typeof(JoinCommand),
            [typeof(LeaveLobbyRequest)]   = typeof(LeaveCommand),
            [typeof(CloseLobbyRequest)]   = typeof(CloseCommand),
            [typeof(InviteMemberRequest)] = typeof(InviteCommand),
            [typeof(KickMemberRequest)]   = typeof(KickCommand),
            [typeof(SetOwnerRequest)]     = typeof(SetOwnerCommand),
            [typeof(LobbyDataRequest)]    = typeof(SetLobbyDataCommand),
            [typeof(MemberDataRequest)]   = typeof(SetMemberDataCommand),

            // Friends
            [typeof(QueryFriendsRequest)] = typeof(QueryFriendsCommand),

            // Browsing
            [typeof(BrowseRequest)] = typeof(BrowseCommand),
            [typeof(ApplyNumberFilterRequest)] = typeof(BrowserNumberFilterCommand),
            [typeof(ApplyStringFilterRequest)] = typeof(BrowserStringFilterCommand),
            [typeof(ApplySlotsAvailableFilterRequest)] = typeof(BrowserSlotsAvailableFilterCommand),
            [typeof(ApplyLimitResponsesFilterRequest)] = typeof(BrowserLimitResponsesFilterCommand),

            // Chat
            [typeof(ChatMessageRequest)] = typeof(ChatMessageCommand),
            [typeof(DirectMessageRequest)] = typeof(DirectMessageCommand);
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