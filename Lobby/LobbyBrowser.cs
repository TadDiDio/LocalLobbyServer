using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer;

public class LobbyBrowser
{
    private LobbyManager _manager;

    public LobbyBrowser(LobbyManager manager)
    {
        _manager = manager;
    }

    private const int DefaultMaxResults = 50;

    private List<ApplyNumberFilterRequest> _numberFilters = [];
    private List<ApplyStringFilterRequest> _stringFilters = [];
    private int _minSlotsAvailable = 0;
    private int _maxResults = DefaultMaxResults;

    public void ApplyNumberFilter(ApplyNumberFilterRequest request)
    {
       _numberFilters.Add(request);
    }
    
    public void ApplyStringFilter(ApplyStringFilterRequest request)
    {
        _stringFilters.Add(request);
    }
    
    public void ApplySlotsAvailableFilter(ApplySlotsAvailableFilterRequest request)
    {
        _minSlotsAvailable = request.Min;
    }
    
    public void ApplyLimitResponsesFilter(ApplyLimitResponsesFilterRequest request)
    {
        _maxResults = request.Max;
    }

    public List<LobbySnapshot> GetLobbies()
    {
        var lobbies = _manager.GetAllLobbies();
    
        var result = new List<LobbySnapshot>();

        foreach (var lobby in lobbies)
        {

            bool failedNum = false;
            foreach (var filter in _numberFilters)
            {
                if (!lobby.LobbyData.ContainsKey(filter.Key) || !int.TryParse(lobby.LobbyData[filter.Key], out var actual))
                {
                    failedNum = true;
                    break;
                }
                
                var test = filter.Value;
                switch (filter.ComparisonType)
                {
                    case 0: if (test == actual) failedNum = true;
                    break;
                    case 1: if (test <= actual) failedNum = true;
                    break;
                    case 2: if (test < actual) failedNum = true;
                    break;
                    case 3: if (test != actual) failedNum = true;
                    break;
                    case 4: if (test >= actual) failedNum = true;
                    break;
                    case 5: if (test > actual) failedNum = true;
                    break;
                }

                if (failedNum) break;
            }

            if (failedNum) continue;

            bool failedString = false;
            foreach (var filter in _stringFilters)
            {
                if (!lobby.LobbyData.TryGetValue(filter.Key,  out var actual) || !filter.Value.Equals(actual)) 
                {
                    failedString = true;
                    break;
                }
            }

            if (failedString) continue;

            var slots = lobby.Capacity - lobby.Members.Count;
            if (_minSlotsAvailable != 0 && slots < _minSlotsAvailable) continue;

            result.Add(lobby);
        }

        result = result.Take(_maxResults).ToList();

        _numberFilters.Clear();
        _stringFilters.Clear();
        _minSlotsAvailable = 0;
        _maxResults = DefaultMaxResults;

        return result;
    }
}