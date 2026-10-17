using System;
using Game.Domain.Models.Dto;
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
        [Header("Menus")] 
        [SerializeField] private PanelRenderer _mainMenu;
        [SerializeField] private PanelRenderer _friendsMenu;
        [SerializeField] private PanelRenderer _notificationsMenu;
        [SerializeField] private PanelRenderer _difficultyMenu;

        [Header("UI Templates")] 
        [SerializeField] private VisualTreeAsset _gameRequest;
        [SerializeField] private VisualTreeAsset _friendRequest;
        [SerializeField] private VisualTreeAsset _friend;

        [Header("Game Logic")] 
        [SerializeField] private GameStarter _gameStarter;

        private readonly StateMachine _stateMachine = new();
        private RequestView _friends;
        private RequestView _friendRequests;
        private RequestView _gameRequests;

        public event Action<string> FriendRequestSent;
        public event Action<User> FriendRequestAccepted;
        public event Action<string> GameRequestSent;
        public event Action<User> GameRequestAccepted;

        public void ShowFriend(User user) => _friends.Add(user);
        public void RemoveFriend(User user) => _friends.Remove(user.Id);
        public void ShowFriendRequest(User user) => _friendRequests.Add(user);
        public void RemoveFriendRequest(User user) => _friendRequests.Remove(user.Id);
        public void ShowGameRequest(User user) => _gameRequests.Add(user);
        public void RemoveGameRequest(User user) => _gameRequests.Remove(user.Id);

        public void HideMenus()
        {
            _mainMenu.gameObject.SetActive(false);
            _friendsMenu.gameObject.SetActive(false);
            _notificationsMenu.gameObject.SetActive(false);
            _difficultyMenu.gameObject.SetActive(false);
        }

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

            ScrollView friendsScrollView = root.Q<ScrollView>("FriendsScrollView");
            friendsScrollView.Clear();
            _friends = new RequestView(friendsScrollView, CreateFriend);
            root.Q<Button>("SendFriendRequestButton").clicked += SendFriendRequest;

            VisualElement CreateFriend(User user)
            {
                VisualElement friend = _friend.Instantiate();
                
                if (friend == null) throw new NullReferenceException("Friend is null");
                if (friend.Q<Label>("Name") == null) throw new NullReferenceException("Friend name is null");
                
                friend.Q<Label>("FriendName").text = user.Name;
                friend.Q<Button>("Play").clicked += () => GameRequestSent?.Invoke(user.Id);
                return friend;
            }

            void SendFriendRequest()
            {
                TextField input = root.Q<TextField>("FriendRequestInput");
                string friendName = input.value.Trim();
                if (string.IsNullOrEmpty(friendName)) return;
                input.value = string.Empty;
                FriendRequestSent?.Invoke(friendName);
            }
        }

        private void RegisterNotificationsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.NotificationsMenu, menu);
            root.Q<Button>("Back").clicked += () => _stateMachine.GoTo(UIState.MainMenu);
            
            ScrollView gameRequestsScrollView = root.Q<ScrollView>("GameRequestsScrollView");
            ScrollView friendRequestsScrollView = root.Q<ScrollView>("FriendRequestsScrollView");
            gameRequestsScrollView.Clear();
            friendRequestsScrollView.Clear();
            _gameRequests = new RequestView(gameRequestsScrollView, CreateGameRequest);
            _friendRequests = new RequestView(friendRequestsScrollView, CreateFriendRequest);

            VisualElement CreateFriendRequest(User user)
            {
                VisualElement request = _friendRequest.Instantiate();
                request.Q<Label>("Name").text = user.Name;
                request.Q<Button>("Accept").clicked += () => FriendRequestAccepted?.Invoke(user);
                request.Q<Button>("Decline").clicked += () =>
                {
                    RemoveFriendRequest(user);
                };
                return request;
            }

            VisualElement CreateGameRequest(User user)
            {
                VisualElement request = _gameRequest.Instantiate();
                request.Q<Label>("Name").text = user.Name;
                request.Q<Button>("Accept").clicked += () =>
                {
                    RemoveGameRequest(user);
                    GameRequestAccepted?.Invoke(user);
                };
                request.Q<Button>("Decline").clicked += () =>
                {
                    RemoveGameRequest(user);
                };
                return request;
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