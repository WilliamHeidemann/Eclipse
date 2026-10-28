using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain.Models.Dto;
using UnityEngine.UIElements;

namespace Game.Presentation.UI;

public class RequestView(ScrollView scrollView, Func<User, VisualElement> createRequest)
{
    private readonly Dictionary<string, VisualElement> _requests = new();

    public void Add(User user)
    {
        if (_requests.ContainsKey(user.Id)) return;

        VisualElement request = createRequest(user);
        scrollView.Add(request);
        _requests.Add(user.Id, request);
    }

    public void Set(IReadOnlyCollection<User> users)
    {
        HashSet<string> ids = new(users.Select(u => u.Id));
        foreach (string staleId in _requests.Keys.Where(id => !ids.Contains(id)).ToList())
        {
            Remove(staleId);
        }

        foreach (User user in users)
        {
            Add(user);
        }
    }

    public void Remove(string friendId)
    {
        if (_requests.TryGetValue(friendId, out VisualElement request))
        {
            scrollView.Remove(request);
            _requests.Remove(friendId);
        }
    }
}