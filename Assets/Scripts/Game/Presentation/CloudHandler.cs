using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain.Models.Dto;
using Game.Presentation.UI;
using Newtonsoft.Json;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.Subscriptions;
using Unity.Services.Core;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using Unity.Services.Friends.Notifications;
using UnityEngine;

namespace Game.Presentation
{
    public class CloudHandler(UIManager uiManager, GameStarter gameStarter)
    {
        private readonly SemaphoreSlim _syncLock = new(1, 1);
        private SynchronizationContext _mainThread;

        public async Task Initialize()
        {
            _mainThread = SynchronizationContext.Current;
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await FriendsService.Instance.InitializeAsync();
            await CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(CreateSubscriptionEventCallbacks());

            BindUI();
            FriendsService.Instance.RelationshipAdded += OnRelationshipAdded;
            FriendsService.Instance.RelationshipDeleted += OnRelationshipDeleted;

            await SyncRelationships();
            string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            uiManager.SetPlayerName(playerName);
        }

        private void BindUI()
        {
            uiManager.FriendRequestSent += friendName => RunCloudOperation(
                FriendsService.Instance.AddFriendByNameAsync(friendName));
            uiManager.PlayerNameChanged += name => RunCloudOperation(ChangePlayerName(name));
            uiManager.FriendRequestAccepted += user => RunCloudOperation(AcceptFriendRequest(user));
            uiManager.FriendRemoved += user => RunCloudOperation(RemoveFriend(user));
            uiManager.FriendRequestDeclined += user => RunCloudOperation(DeclineFriendRequest(user));
            uiManager.GameRequestSent += receiverPlayerId => RunCloudOperation(
                SendGameRequest(AuthenticationService.Instance.PlayerName, receiverPlayerId));
            uiManager.GameRequestAccepted += user => RunCloudOperation(StartMatch(user));
            
            uiManager.FriendRequestSent += friendName => Debug.Log($"Friend request sent to {friendName}");
            uiManager.FriendRequestAccepted += user => Debug.Log($"Accepted friend request from {user}.");
            uiManager.GameRequestSent += receiverPlayerId => Debug.Log($"Sent game request to {receiverPlayerId}.");
            uiManager.GameRequestAccepted += user => Debug.Log($"Accepted game request from {user}.");
        }

        private async Task ChangePlayerName(string newName)
        {
            string updatedName = await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            uiManager.SetPlayerName(updatedName);
        }

        private async Task RemoveFriend(User user)
        {
            await FriendsService.Instance.DeleteFriendAsync(user.Id);
            await SyncRelationships();
        }

        private async Task DeclineFriendRequest(User user)
        {
            await FriendsService.Instance.DeleteIncomingFriendRequestAsync(user.Id);
            await SyncRelationships();
        }

        private async Task AcceptFriendRequest(User user)
        {
            await FriendsService.Instance.AddFriendByNameAsync(user.Name);
            await SyncRelationships();
        }

        private SubscriptionEventCallbacks CreateSubscriptionEventCallbacks()
        {
            SubscriptionEventCallbacks callbacks = new();
            callbacks.MessageReceived += async messageReceivedEvent =>
                await HandlePushMessageReceivedEvent(messageReceivedEvent);
            callbacks.Error += Debug.LogError;
            return callbacks;
        }

        private async Task StartMatch(User opponent)
        {
            string opponentId = opponent.Id;
            Match match = await gameStarter.CreateMatch(opponentId);
            gameStarter.StartGame(match, opponentId);
            uiManager.ShowPlayerNames(match.ClientGameState.Team, opponent.Name);

            PushMessagesServiceBindings pushMessagesServiceModule = new();
            bool success = await pushMessagesServiceModule.AcceptChallenge(match.Id, opponentId);
            if (success) Debug.Log("Push message sent: Match accepted.");
            else Debug.LogError("Failed to send push message: Match accepted.");
            uiManager.HideMenus();
        }

        private async Task HandlePushMessageReceivedEvent(
            Unity.Services.CloudCode.Subscriptions.IMessageReceivedEvent messageReceivedEvent)
        {
            if (!Enum.TryParse(messageReceivedEvent.MessageType, true, out PushMessageType messageType))
            {
                Debug.LogError($"{messageReceivedEvent.MessageType} is not a valid push message type");
                return;
            }

            Debug.Log($"[{messageReceivedEvent.MessageType}] {messageReceivedEvent.Message}");
            switch (messageType)
            {
                case PushMessageType.ChallengeRequest:
                    User challenger = JsonConvert.DeserializeObject<User>(messageReceivedEvent.Message);
                    uiManager.ShowGameRequest(challenger);
                    break;
                case PushMessageType.ChallengeAccepted:
                    MatchAccepted matchAccepted =
                        JsonConvert.DeserializeObject<MatchAccepted>(messageReceivedEvent.Message);
                    GameLogicServiceBindings gameLogicServiceModule = new();
                    var clientGameState = await gameLogicServiceModule.GetClientGameState(matchAccepted.MatchID);
                    Match match = new()
                    {
                        ClientGameState = clientGameState.ToModel(),
                        Id = matchAccepted.MatchID,
                    };
                    gameStarter.StartGame(match, matchAccepted.OpponentID);
                    uiManager.ShowPlayerNames(match.ClientGameState.Team, FindFriendName(matchAccepted.OpponentID));
                    uiManager.GoToMenu(UIState.GameUI);
                    break;
                case PushMessageType.NewMove:
                    await gameStarter.OnNewMovePushMessageReceived(messageReceivedEvent.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string FindFriendName(string playerId)
        {
            Relationship friend = FriendsService.Instance.Friends.FirstOrDefault(r => r.Member.Id == playerId);
            string name = friend?.Member.Profile?.Name;
            return string.IsNullOrWhiteSpace(name) ? "Opponent" : name;
        }

        private static async Task SendGameRequest(string challengerName, string receiverPlayerId)
        {
            PushMessagesServiceBindings pushMessagesServiceModule = new();
            bool success = await pushMessagesServiceModule.ChallengeFriend(challengerName, receiverPlayerId);
            Debug.Log(success ? "Challenge Friend Success" : "Challenge Friend Failed");
        }

        // Notifications may arrive off the main thread, without profiles and in any order,
        // so every relationship change re-reads the server state and syncs the UI with it.
        private void OnRelationshipDeleted(IRelationshipDeletedEvent relationshipDeletedEvent)
        {
            _mainThread.Post(_ => RunCloudOperation(SyncRelationships()), null);
        }

        private void OnRelationshipAdded(IRelationshipAddedEvent relationshipAddedEvent)
        {
            _mainThread.Post(_ => RunCloudOperation(SyncRelationships()), null);
        }

        private async Task SyncRelationships()
        {
            await _syncLock.WaitAsync();
            try
            {
                await FriendsService.Instance.ForceRelationshipsRefreshAsync();
                List<User> friends = FriendsService.Instance.Friends.Select(r => ToUser(r.Member)).ToList();
                List<User> requests = FriendsService.Instance.IncomingFriendRequests
                    .Select(r => ToUser(r.Member)).ToList();
                Debug.Log($"Relationships synced: {friends.Count} friends, {requests.Count} incoming requests.");
                uiManager.SetFriends(friends);
                uiManager.SetFriendRequests(requests);
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private static User ToUser(Member member)
        {
            return new User
            {
                Id = member.Id,
                Name = member.Profile?.Name ?? string.Empty
            };
        }

        private static async void RunCloudOperation(Task operation)
        {
            try
            {
                await operation;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
