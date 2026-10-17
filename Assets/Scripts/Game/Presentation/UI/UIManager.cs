using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PanelRenderer _mainMenu;
        [SerializeField] private PanelRenderer _friendsMenu;
        [SerializeField] private PanelRenderer _notificationsMenu;
        [SerializeField] private PanelRenderer _difficultyMenu;

        public record UIRoots(
            VisualElement MainMenuRoot,
            VisualElement FriendsRoot,
            VisualElement NotificationsRoot,
            VisualElement DifficultyRoot);
        
        public class UIStateMachine
        {
            private UIState _state;
            private readonly UIRoots _roots;

            public UIStateMachine(UIRoots roots)
            {
                _roots = roots;
            }

            public void GoTo(UIState state)
            {
                if (state == _state) return;
                
                VisualElement old = GetRoot(_state);
                old.style.display = DisplayStyle.None;

                VisualElement newRoot = GetRoot(state);
                newRoot.style.display = DisplayStyle.Flex;
                
                _state = state;
            }

            private VisualElement GetRoot(UIState state)
            {
                return state switch
                {
                    UIState.MainMenu => _roots.MainMenuRoot,
                    UIState.FriendsMenu => _roots.FriendsRoot,
                    UIState.NotificationsMenu => _roots.NotificationsRoot,
                    UIState.DifficultyMenu => _roots.DifficultyRoot,
                    _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                };
            }
        }

        public enum UIState
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
            // root.Q<Button>("Bot").clicked += OpenDifficulty;
        }
        
        private void RegisterFriendsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
        }
        
        private void RegisterNotificationsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
        }

        private void RegisterDifficultyMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
        }
    }
}