using System;
using System.Linq;
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
        public async Task Initialize()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await FriendsService.Instance.InitializeAsync();
            await CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(CreateSubscriptionEventCallbacks());

            BindUI();
            FriendsService.Instance.RelationshipAdded += OnRelationshipAdded;
            FriendsService.Instance.RelationshipDeleted += OnRelationshipDeleted;

            FriendsService.Instance.Friends.Select(r => ToUser(r.Member))
                .ToList()
                .ForEach(uiManager.ShowFriend);
            FriendsService.Instance.IncomingFriendRequests.Select(r => ToUser(r.Member))
                .ToList()
                .ForEach(uiManager.ShowFriendRequest);
        }

        private void BindUI()
        {
            uiManager.FriendRequestSent += friendName => RunCloudOperation(
                FriendsService.Instance.AddFriendByNameAsync(friendName));
            uiManager.FriendRequestAccepted += user => RunCloudOperation(AcceptFriendRequest(user));
            uiManager.GameRequestSent += receiverPlayerId => RunCloudOperation(
                SendGameRequest(AuthenticationService.Instance.PlayerName, receiverPlayerId));
            uiManager.GameRequestAccepted += user => RunCloudOperation(StartMatch(user.Id));
            
            uiManager.FriendRequestSent += friendName => Debug.Log($"Friend request sent to {friendName}");
            uiManager.FriendRequestAccepted += user => Debug.Log($"Accepted friend request from {user}.");
            uiManager.GameRequestSent += receiverPlayerId => Debug.Log($"Sent game request to {receiverPlayerId}.");
            uiManager.GameRequestAccepted += user => Debug.Log($"Accepted game request from {user}.");
        }

        private async Task AcceptFriendRequest(User user)
        {
            await FriendsService.Instance.AddFriendByNameAsync(user.Name);
            uiManager.RemoveFriendRequest(user);
        }

        private SubscriptionEventCallbacks CreateSubscriptionEventCallbacks()
        {
            SubscriptionEventCallbacks callbacks = new();
            callbacks.MessageReceived += async messageReceivedEvent =>
                await HandlePushMessageReceivedEvent(messageReceivedEvent);
            callbacks.Error += Debug.LogError;
            return callbacks;
        }

        private async Task StartMatch(string opponentId)
        {
            Match match = await gameStarter.CreateMatch(opponentId);
            gameStarter.StartGame(match, opponentId);

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
                    uiManager.HideMenus();
                    break;
                case PushMessageType.NewMove:
                    await gameStarter.OnNewMovePushMessageReceived(messageReceivedEvent.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task SendGameRequest(string challengerName, string receiverPlayerId)
        {
            PushMessagesServiceBindings pushMessagesServiceModule = new();
            bool success = await pushMessagesServiceModule.ChallengeFriend(challengerName, receiverPlayerId);
            Debug.Log(success ? "Challenge Friend Success" : "Challenge Friend Failed");
        }

        private void OnRelationshipDeleted(IRelationshipDeletedEvent relationshipDeletedEvent)
        {
            User user = ToUser(relationshipDeletedEvent.Relationship.Member);
            switch (relationshipDeletedEvent.Relationship.Type)
            {
                case RelationshipType.Friend:
                    uiManager.RemoveFriend(user);
                    break;
                case RelationshipType.FriendRequest:
                    uiManager.RemoveFriendRequest(user);
                    break;
                case RelationshipType.Block:
                    Debug.LogWarning("Block relationship deletion is not handled by the UI.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnRelationshipAdded(IRelationshipAddedEvent relationshipAddedEvent)
        {
            User user = ToUser(relationshipAddedEvent.Relationship.Member);
            switch (relationshipAddedEvent.Relationship.Type)
            {
                case RelationshipType.Friend:
                    uiManager.ShowFriend(user);
                    break;
                case RelationshipType.FriendRequest:
                    uiManager.ShowFriendRequest(user);
                    break;
                case RelationshipType.Block:
                    Debug.LogWarning("Block relationships are not displayed in the UI.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static User ToUser(Member member)
        {
            return new User
            {
                Id = member.Id,
                Name = member.Profile.Name
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
