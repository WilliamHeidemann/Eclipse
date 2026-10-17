using System;
using System.Collections.Generic;
using Game.Presentation.UI.Menus;

namespace Game.Presentation.UI;

public class StateMachine
{
    private UIState _state = UIState.MainMenu;
    private readonly Dictionary<UIState, IMenu> _menus = new();

    public void GoTo(UIState state)
    {
        if (state == _state) return;
        if (!_menus.ContainsKey(state)) throw new Exception($"State {state} not registered");

        IMenu previous = _menus[_state];
        previous.Hide();

        IMenu next = _menus[state];
        next.Show();

        _state = state;
    }

    public void Register(UIState state, IMenu menu)
    {
        _menus[state] = menu;
    }
}