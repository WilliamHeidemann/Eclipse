using System;
using FMODUnity;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
using Game.Presentation.AnimationSystems;
using Game.Presentation.Audio;
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
        [Header("Audio")]
        [SerializeField] private UIAudioConfig _audioConfig;

        [Header("Menus")] 
        [SerializeField] private PanelRenderer _mainMenu;
        [SerializeField] private PanelRenderer _friendsMenu;
        [SerializeField] private PanelRenderer _difficultyMenu;
        [SerializeField] private PanelRenderer _gameUI;
        [SerializeField] private PanelRenderer _creditsMenu;
        [SerializeField] private PanelRenderer _gameOverMenu;

        [Header("UI Templates")] 
        [SerializeField] private VisualTreeAsset _gameRequest;
        [SerializeField] private VisualTreeAsset _friendRequest;
        [SerializeField] private VisualTreeAsset _friend;

        [Header("Game Logic")] 
        [SerializeField] private GameStarter _gameStarter;
        [SerializeField] private AnimationOrchestrator _animationOrchestrator;

        private readonly StateMachine _stateMachine = new();
        private RequestView _friends;
        private RequestView _friendRequests;
        private RequestView _gameRequests;
        private Label _playerName;
        private TextField _playerNameInput;
        private Button _editPlayerNameButton;

        public event Action<string> FriendRequestSent;
        public event Action<User> FriendRequestAccepted;
        public event Action<User> FriendRemoved;
        public event Action<User> FriendRequestDeclined;
        public event Action<string> PlayerNameChanged;
        public event Action<string> GameRequestSent;
        public event Action<User> GameRequestAccepted;

        private const float InactivePlayerOpacity = 0.4f;
        private VisualElement _playerOneRow;
        private VisualElement _playerTwoRow;
        private Team _currentTurn = Team.Red;

        public void ShowTurn(Team toPlay)
        {
            _currentTurn = toPlay;
            if (_playerOneRow == null || _playerTwoRow == null) return;

            _playerOneRow.style.opacity = toPlay == Team.Red ? 1f : InactivePlayerOpacity;
            _playerTwoRow.style.opacity = toPlay == Team.Yellow ? 1f : InactivePlayerOpacity;
        }

        private string _localPlayerName = DefaultPlayerName;
        private string _redName = DefaultPlayerName;
        private string _yellowName = "Opponent";
        private Label _playerOneLabel;
        private Label _playerTwoLabel;

        private const string DefaultPlayerName = "Player";
        private const int MaxPlayerNameLength = 10;

        public void ShowPlayerNames(Team localTeam, string opponentName)
        {
            string localName = _localPlayerName;
            _redName = localTeam == Team.Red ? localName : opponentName;
            _yellowName = localTeam == Team.Red ? opponentName : localName;
            ApplyPlayerNames();
        }

        public void ShowGameOver(Team winner)
        {
            _winnerName = winner == Team.Red ? _redName : _yellowName;
            if (_winnerLabel != null) _winnerLabel.text = _winnerName;
            _stateMachine.GoTo(UIState.GameOverMenu);
        }

        private string _winnerName = DefaultPlayerName;
        private Label _winnerLabel;

        private void ApplyPlayerNames()
        {
            if (_playerOneLabel != null) _playerOneLabel.text = _redName;
            if (_playerTwoLabel != null) _playerTwoLabel.text = _yellowName;
        }

        private static string BotName(BotDifficulty difficulty) => $"{difficulty} Bot";

        public void SetPlayerName(string playerName)
        {
            _localPlayerName = string.IsNullOrWhiteSpace(playerName) ? DefaultPlayerName : playerName;
            if (_playerName != null) _playerName.text = playerName;
        }

        private void BeginEditPlayerName()
        {
            string current = _playerName.text ?? string.Empty;
            int suffixStart = current.IndexOf('#');
            string baseName = suffixStart >= 0 ? current[..suffixStart] : current;
            _playerNameInput.value = baseName.Trim();
            _playerName.style.display = DisplayStyle.None;
            _editPlayerNameButton.style.display = DisplayStyle.None;
            _playerNameInput.style.display = DisplayStyle.Flex;
            _playerNameInput.Focus();
        }

        private void EndEditPlayerName(bool commit)
        {
            if (_playerNameInput.style.display == DisplayStyle.None) return;

            string newName = _playerNameInput.value.Trim().Replace(" ", string.Empty);
            if (newName.Length > MaxPlayerNameLength) newName = newName[..MaxPlayerNameLength];
            _playerNameInput.style.display = DisplayStyle.None;
            _playerName.style.display = DisplayStyle.Flex;
            _editPlayerNameButton.style.display = DisplayStyle.Flex;

            bool changed = commit && newName.Length > 0 && !_playerName.text.StartsWith(newName + "#");
            if (!changed) return;

            PlaySound(UISound.ButtonClick);
            PlayerNameChanged?.Invoke(newName);
        }

        public void SetFriends(System.Collections.Generic.IReadOnlyCollection<User> users) => _friends.Set(users);
        public void SetFriendRequests(System.Collections.Generic.IReadOnlyCollection<User> users) => _friendRequests.Set(users);
        public void ShowGameRequest(User user) => _gameRequests.Add(user);
        public void RemoveGameRequest(User user) => _gameRequests.Remove(user.Id);

        public void HideMenus()
        {
            _mainMenu.gameObject.SetActive(false);
            _friendsMenu.gameObject.SetActive(false);
            _difficultyMenu.gameObject.SetActive(false);
            _creditsMenu.gameObject.SetActive(false);
        }

        private void PlaySound(UISound sound)
        {
            if (_audioConfig == null) return;

            EventReference evt = _audioConfig.Get(sound);
            if (!evt.IsNull)
            {
                RuntimeManager.PlayOneShot(evt);
            }
        }

        public void GoToMenu(UIState menuToGoTo)
        {
            _stateMachine.GoTo(menuToGoTo);
            if (menuToGoTo == UIState.GameUI)
            {
                _animationOrchestrator.ShowStaticGameElements();
            }
        }

        private void Awake()
        {
            _mainMenu.gameObject.SetActive(true);
            _friendsMenu.gameObject.SetActive(true);
            _difficultyMenu.gameObject.SetActive(true);
            _creditsMenu.gameObject.SetActive(true);
            _gameUI.gameObject.SetActive(true);
            _gameOverMenu.gameObject.SetActive(true);

            _gameOverMenu.RegisterUIReloadCallback(RegisterGameOverMenu);
            _mainMenu.RegisterUIReloadCallback(RegisterMainMenu);
            _friendsMenu.RegisterUIReloadCallback(RegisterFriendsMenu);
            _difficultyMenu.RegisterUIReloadCallback(RegisterDifficultyMenu);
            _creditsMenu.RegisterUIReloadCallback(RegisterCreditsMenu);
            _gameUI.RegisterUIReloadCallback(RegisterGameUI);
        }

        private void OnDestroy()
        {
            _gameOverMenu.UnregisterUIReloadCallback(RegisterGameOverMenu);
            _mainMenu.UnregisterUIReloadCallback(RegisterMainMenu);
            _friendsMenu.UnregisterUIReloadCallback(RegisterFriendsMenu);
            _difficultyMenu.UnregisterUIReloadCallback(RegisterDifficultyMenu);
            _creditsMenu.UnregisterUIReloadCallback(RegisterCreditsMenu);
            _gameUI.UnregisterUIReloadCallback(RegisterGameUI);
        }

        private void RegisterMainMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.Flex;
            VerticalShiftMenu menu = new(root, false);
            _stateMachine.Register(UIState.MainMenu, menu);

            root.Q<Button>("Bot").clicked += () =>
            {   
                PlaySound(UISound.ButtonClick);
                PlaySound(UISound.MenuTransition);
                _stateMachine.GoTo(UIState.DifficultyMenu);
            };

            root.Q<Button>("Friends").clicked += () =>
            {   
                PlaySound(UISound.ButtonClick);
                PlaySound(UISound.MenuTransition);
                _stateMachine.GoTo(UIState.FriendsMenu);
            };

            root.Q<Button>("Credits").clicked += () =>
            {
                PlaySound(UISound.ButtonClick);
                PlaySound(UISound.MenuTransition);
                _stateMachine.GoTo(UIState.CreditsMenu);
            };
        }

        private void RegisterFriendsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.FriendsMenu, menu);

            root.Q<Button>("Back").clicked += () =>
            {   
                PlaySound(UISound.MenuTransitionBack);
                PlaySound(UISound.BackClick);
                _stateMachine.GoTo(UIState.MainMenu);
            };

            ScrollView friendsScrollView = root.Q<ScrollView>("FriendsScrollView");
            friendsScrollView.Clear();
            _friends = new RequestView(friendsScrollView, CreateFriend);
            root.Q<Button>("SendFriendRequestButton").clicked += SendFriendRequest;

            ScrollView friendRequestsScrollView = root.Q<ScrollView>("FriendRequestsScrollView");
            ScrollView gameRequestsScrollView = root.Q<ScrollView>("GameRequestsScrollView");
            friendRequestsScrollView.Clear();
            gameRequestsScrollView.Clear();
            _friendRequests = new RequestView(friendRequestsScrollView, CreateFriendRequest);
            _gameRequests = new RequestView(gameRequestsScrollView, CreateGameRequest);
            _playerName = root.Q<Label>("PlayerName");
            _playerNameInput = root.Q<TextField>("PlayerNameInput");
            _editPlayerNameButton = root.Q<Button>("EditPlayerNameButton");
            _playerNameInput.maxLength = MaxPlayerNameLength;
            _editPlayerNameButton.clicked += BeginEditPlayerName;
            _playerNameInput.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter) EndEditPlayerName(true);
                else if (evt.keyCode == KeyCode.Escape) EndEditPlayerName(false);
            });
            _playerNameInput.RegisterCallback<BlurEvent>(_ => EndEditPlayerName(true));

            VisualElement CreateFriend(User user)
            {
                TemplateContainer friend = _friend.Instantiate();
                
                if (friend == null) throw new NullReferenceException("Friend is null");
                if (friend.Q<Label>("FriendName") == null) throw new NullReferenceException("Friend name label is null");
                
                friend.Q<Label>("FriendName").text = user.Name;
                friend.Q<Button>("Play").clicked += () =>
                {
                    PlaySound(UISound.RequestSent);
                    GameRequestSent?.Invoke(user.Id);
                };
                friend.Q<Button>("Remove").clicked += () =>
                {
                    PlaySound(UISound.ButtonClick);
                    FriendRemoved?.Invoke(user);
                };
                return friend;
            }

            void SendFriendRequest()
            {
                TextField input = root.Q<TextField>("FriendRequestInput");
                string friendName = input.value.Trim();
                if (string.IsNullOrEmpty(friendName)) return;

                PlaySound(UISound.RequestSent);
                input.value = string.Empty;
                FriendRequestSent?.Invoke(friendName);
            }

            VisualElement CreateFriendRequest(User user)
            {
                VisualElement request = _friendRequest.Instantiate();
                request.Q<Label>("Name").text = user.Name;
                request.Q<Button>("Accept").clicked += () =>
                {
                    PlaySound(UISound.RequestAccepted);
                    FriendRequestAccepted?.Invoke(user);
                };
                request.Q<Button>("Decline").clicked += () =>
                {
                    PlaySound(UISound.ButtonClick);
                    FriendRequestDeclined?.Invoke(user);
                };
                return request;
            }

            VisualElement CreateGameRequest(User user)
            {
                VisualElement request = _gameRequest.Instantiate();
                request.Q<Label>("Name").text = user.Name;
                request.Q<Button>("Accept").clicked += () =>
                {
                    PlaySound(UISound.RequestAccepted);
                    RemoveGameRequest(user);
                    _stateMachine.GoTo(UIState.GameUI);
                    _animationOrchestrator.ShowStaticGameElements();
                    GameRequestAccepted?.Invoke(user);
                };
                request.Q<Button>("Decline").clicked += () =>
                {
                    PlaySound(UISound.ButtonClick);
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

            root.Q<Button>("Back").clicked += () =>
            {
                PlaySound(UISound.BackClick);
                PlaySound(UISound.MenuTransitionBack);
                _stateMachine.GoTo(UIState.MainMenu);
            };

            root.Q<Button>("Easy").clicked += () => StartGame(BotDifficulty.Easy);
            root.Q<Button>("Hard").clicked += () => StartGame(BotDifficulty.Hard);
            root.Q<Button>("Extreme").clicked += () => StartGame(BotDifficulty.Extreme);
            return;

            void StartGame(BotDifficulty difficulty)
            {
                PlaySound(UISound.ButtonClick);
                root.style.display = DisplayStyle.None;
                _stateMachine.GoTo(UIState.GameUI);
                _gameStarter.StartLocalGame(difficulty);
                ShowPlayerNames(Team.Red, BotName(difficulty));
                _animationOrchestrator.ShowStaticGameElements();
            }
        }

        private void RegisterCreditsMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.CreditsMenu, menu);

            root.Q<Button>("Back").clicked += () =>
            {   
                PlaySound(UISound.MenuTransitionBack);
                PlaySound(UISound.BackClick);
                _stateMachine.GoTo(UIState.MainMenu);
            };
        }

        private void RegisterGameOverMenu(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            VerticalShiftMenu menu = new(root, true);
            _stateMachine.Register(UIState.GameOverMenu, menu);

            _winnerLabel = root.Q<Label>("Winner");
            _winnerLabel.text = _winnerName;

            root.Q<Button>("Back").clicked += BackToMainMenu;
            root.Q<Button>("BackHome").clicked += BackToMainMenu;
            root.Q<Button>("Rematch").clicked += () => throw new NotImplementedException();

            void BackToMainMenu()
            {
                PlaySound(UISound.MenuTransitionBack);
                PlaySound(UISound.BackClick);
                _stateMachine.GoTo(UIState.MainMenu);
                _animationOrchestrator.HideGameElements();
            }
        }

        private void RegisterGameUI(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            root.style.display = DisplayStyle.None;
            SimpleDisplayMenu menu = new(root);
            _stateMachine.Register(UIState.GameUI, menu);

            _playerOneRow = root.Q<VisualElement>("PlayerOneRow");
            _playerTwoRow = root.Q<VisualElement>("PlayerTwoRow");
            _playerOneLabel = root.Q<Label>("PlayerOneLabel");
            _playerTwoLabel = root.Q<Label>("PlayerTwoLabel");
            ApplyPlayerNames();
            ShowTurn(_currentTurn);

            root.Q<Button>("Back").clicked += HideGameUI;
            void HideGameUI()
            {
                _stateMachine.GoTo(UIState.MainMenu);
                _animationOrchestrator.HideGameElements();
            }
        }
    }
}