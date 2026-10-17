using System;
using System.Collections.Generic;
using Game.Presentation.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;
using UtilityToolkit.Editor;

namespace Game.Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PanelRenderer _mainMenu;
        [SerializeField] private PanelRenderer _friendsMenu;
        [SerializeField] private PanelRenderer _notificationsMenu;
        [SerializeField] private PanelRenderer _difficultyMenu;
        [SerializeField] private GameStarter _gameStarter;

        private readonly StateMachine _stateMachine = new();

        [Header("Debugging")] [SerializeField]
        private UIState _menuToGoTo;
        
        [Button]
        public void GoToMenu()
        {
            _stateMachine.GoTo(_menuToGoTo);
        }
        
        private class StateMachine
        {
            private UIState _state = UIState.MainMenu;
            private readonly Dictionary<UIState, IMenu> _menus = new();
            
            public void GoTo(UIState state)
            {
                if (state == _state) return;

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

        private enum UIState
        {
            MainMenu,
            FriendsMenu,
            NotificationsMenu,
            DifficultyMenu
        }

        private void Awake()
        {
            _mainMenu.gameObject.SetActive(true);
            _friendsMenu.gameObject.SetActive(true);
            _notificationsMenu.gameObject.SetActive(true);
            _difficultyMenu.gameObject.SetActive(true);

            _mainMenu.RegisterUIReloadCallback(RegisterMainMenu);
            _friendsMenu.RegisterUIReloadCallback(RegisterFriendsMenu);
            _notificationsMenu.RegisterUIReloadCallback(RegisterNotificationsMenu);
            _difficultyMenu.RegisterUIReloadCallback(RegisterDifficultyMenu);
        }

        private void RegisterMainMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.Flex;
            VerticalShiftMenu menu = new(root, false);
            _stateMachine.Register(UIState.MainMenu, menu);
            root.Q<Button>("Bot").clicked += () => _stateMachine.GoTo(UIState.DifficultyMenu);
            root.Q<Button>("Notifications").clicked += () => _stateMachine.GoTo(UIState.NotificationsMenu);
            root.Q<Button>("Friends").clicked += () => _stateMachine.GoTo(UIState.FriendsMenu);
        }

        private void RegisterFriendsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.FriendsMenu, menu);
            root.Q<Button>("Back").clicked += () => _stateMachine.GoTo(UIState.MainMenu);
        }

        private void RegisterNotificationsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.NotificationsMenu, menu);
            root.Q<Button>("Back").clicked += () => _stateMachine.GoTo(UIState.MainMenu);
        }

        private void RegisterDifficultyMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.DifficultyMenu, menu);
            root.Q<Button>("Back").clicked += () => _stateMachine.GoTo(UIState.MainMenu);
            root.Q<Button>("Easy").clicked += () => StartGame(BotDifficulty.Easy);
            root.Q<Button>("Hard").clicked += () => StartGame(BotDifficulty.Hard);
            root.Q<Button>("Extreme").clicked += () => StartGame(BotDifficulty.Extreme);
            return;

            void StartGame(BotDifficulty difficulty)
            {
                root.style.display = DisplayStyle.None;
                _gameStarter.StartLocalGame(difficulty);
            }
        }
    }
}