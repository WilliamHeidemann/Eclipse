using System;
using Game.Presentation.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UtilityToolkit.Editor;
#endif

namespace Game.Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Menus")] [SerializeField] private PanelRenderer _mainMenu;
        [SerializeField] private PanelRenderer _friendsMenu;
        [SerializeField] private PanelRenderer _notificationsMenu;
        [SerializeField] private PanelRenderer _difficultyMenu;

        [Header("UI Templates")] [SerializeField]
        private VisualTreeAsset _gameRequest;

        [SerializeField] private VisualTreeAsset _friendRequest;

        [Header("Game Logic")] [SerializeField]
        private GameStarter _gameStarter;

        private readonly StateMachine _stateMachine = new();
        private RequestView _friendRequests;
        private RequestView _gameRequests;
        
        public void ShowFriendRequest(string friendName) => _friendRequests.Add(friendName);
        public void RemoveFriendRequest(string friendName) => _friendRequests.Remove(friendName);
        public void ShowGameRequest(string friendName) => _gameRequests.Add(friendName);
        public void RemoveGameRequest(string friendName) => _gameRequests.Remove(friendName);

#if UNITY_EDITOR
        [Header("Debugging")] [SerializeField] private UIState _menuToGoTo;
        [Button]
        public void GoToMenu()
        {
            _stateMachine.GoTo(_menuToGoTo);
        }
#endif
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

        private void OnDestroy()
        {
            _mainMenu.UnregisterUIReloadCallback(RegisterMainMenu);
            _friendsMenu.UnregisterUIReloadCallback(RegisterFriendsMenu);
            _notificationsMenu.UnregisterUIReloadCallback(RegisterNotificationsMenu);
            _difficultyMenu.UnregisterUIReloadCallback(RegisterDifficultyMenu);
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

            ScrollView scrollView = root.Q<ScrollView>();
            _friendRequests = new RequestView(scrollView, CreateFriendRequest);
            return;

            VisualElement CreateFriendRequest(string friendName)
            {
                VisualElement request = _friendRequest.Instantiate();
                request.Q<Label>("Name").text = friendName;
                request.Q<Button>("Accept").clicked += AcceptFriend;
                request.Q<Button>("Decline").clicked += DeclineFriend;
                return request;
                
                void AcceptFriend()
                {
                    throw new NotImplementedException();
                }

                void DeclineFriend()
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void RegisterNotificationsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.NotificationsMenu, menu);
            root.Q<Button>("Back").clicked += () => _stateMachine.GoTo(UIState.MainMenu);
            
            ScrollView scrollView = root.Q<ScrollView>();
            _gameRequests = new RequestView(scrollView, CreateGameRequest);
            return;

            VisualElement CreateGameRequest(string friendName)
            {
                VisualElement request = _friendRequest.Instantiate();
                request.Q<Label>("Name").text = friendName;
                request.Q<Button>("Accept").clicked += AcceptMatch;
                request.Q<Button>("Decline").clicked += DeclineMatch;
                return request;
                
                void AcceptMatch()
                {
                    throw new NotImplementedException();
                }

                void DeclineMatch()
                {
                    throw new NotImplementedException();
                }
            }
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