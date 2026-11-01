using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.Presentation.UI;

public class GameListView(VisualElement section, ScrollView scrollView, Func<OngoingGame, VisualElement> createGame)
{
    public bool HasGames { get; private set; }

    public void Set(IReadOnlyCollection<OngoingGame> games)
    {
        scrollView.Clear();
        foreach (OngoingGame game in games)
        {
            scrollView.Add(createGame(game));
        }

        HasGames = games.Count > 0;
        section.style.display = HasGames ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
