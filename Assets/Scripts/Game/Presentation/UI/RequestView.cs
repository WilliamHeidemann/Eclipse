using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.Presentation.UI;

public class RequestView(ScrollView scrollView, Func<string, VisualElement> createRequest)
{
    private readonly Dictionary<string, VisualElement> _requests = new();

    public void Add(string friendName)
    {
        if (_requests.ContainsKey(friendName)) return;

        VisualElement request = createRequest(friendName);
        scrollView.Add(request);
        _requests.Add(friendName, request);
    }

    public void Remove(string friendName)
    {
        if (_requests.TryGetValue(friendName, out VisualElement request))
        {
            scrollView.Remove(request);
        }
    }
}