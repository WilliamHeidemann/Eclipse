using System;
using System.Collections.Generic;
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

    public void Remove(string friendId)
    {
        if (_requests.TryGetValue(friendId, out VisualElement request))
        {
            scrollView.Remove(request);
            _requests.Remove(friendId);
        }
    }
}