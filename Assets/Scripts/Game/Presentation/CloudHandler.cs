using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain.Models.Dto;
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
    public static class CloudHandler
    {
        public static async Task Initialize(MainMenu mainMenu, GameStarter gameStarter)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await FriendsService.Instance.InitializeAsync();
            await CloudCodeService.Instance.SubscribeToPlayerMessagesAsync(
                CreateSubscriptionEventCallbacks(mainMenu, gameStarter));
            BindMainMenu(mainMenu, gameStarter);
        }

        private static void BindMainMenu(MainMenu mainMenu, GameStarter gameStarter)
        {
            FriendsService.Instance.RelationshipAdded += (e) => OnRelationShipAdded(e, mainMenu);
            FriendsService.Instance.RelationshipDeleted += (e) => OnRelationShipDeleted(e, mainMenu);

            mainMenu.SetPlayerName(AuthenticationService.Instance.PlayerName);
            FriendsService.Instance.Friends.Select(r => r.Member).ToList()
                .ForEach(mainMenu.ShowFriend);
            FriendsService.Instance.IncomingFriendRequests.Select(r => r.Member).ToList()
                .ForEach(mainMenu.ShowFriendRequest);

            mainMenu.OnSetPlayerName += async n => await AuthenticationService.Instance.UpdatePlayerNameAsync(n);
            mainMenu.OnSentFriendRequest += async n => await FriendsService.Instance.AddFriendByNameAsync(n);
            mainMenu.OnSentGameRequest +=
                async n => await SendGameRequest(AuthenticationService.Instance.PlayerName, n);
            mainMenu.OnChallengeAccepted += async challengerId => await StartMatch(challengerId, gameStarter, mainMenu);
            
            mainMenu.OnSetPlayerName += Debug.Log;
            mainMenu.OnSentFriendRequest += Debug.Log;
            mainMenu.OnSentGameRequest += Debug.Log;
            mainMenu.OnChallengeAccepted += Debug.Log;
        }

        private static SubscriptionEventCallbacks CreateSubscriptionEventCallbacks(MainMenu mainMenu,
            GameStarter gameStarter)
        {
            SubscriptionEventCallbacks callbacks = new();

            // The subscribed callback should handle all push message types.
            // 1) GameRequest(playerId)
            // 2) OpponentAcceptedGame(matchId)
            // 3) OpponentPlayed(matchId)
            callbacks.MessageReceived += async messageReceivedEvent =>
                await HandlePushMessageReceivedEvent(messageReceivedEvent, mainMenu, gameStarter);

            callbacks.Error += Debug.LogError;

            return callbacks;
        }

        private static async Task StartMatch(string opponentId, GameStarter gameStarter, MainMenu mainMenu)
        {
            Match match = await gameStarter.CreateMatch(opponentId);
            gameStarter.StartGame(match, opponentId);
            PushMessagesServiceBindings pushMessagesServiceModule = new();
            var success = await pushMessagesServiceModule.AcceptChallenge(match.Id, opponentId);
            if (success) Debug.Log("Push message sent: Match accepted.");
            else Debug.LogError("Failed to send push message: Match accepted.");
            mainMenu.gameObject.SetActive(false);
        }

        private static async Task HandlePushMessageReceivedEvent(
            Unity.Services.CloudCode.Subscriptions.IMessageReceivedEvent messageReceivedEvent, MainMenu mainMenu,
            GameStarter gameStarter)
        {
            if (Enum.TryParse(messageReceivedEvent.MessageType, true, out PushMessageType messageType))
            {
                Debug.Log($"[{messageReceivedEvent.MessageType}] {messageReceivedEvent.Message}");
                
                switch (messageType)
                {
                    case PushMessageType.ChallengeRequest:
                        User user = JsonConvert.DeserializeObject<User>(messageReceivedEvent.Message);
                        mainMenu.OpenGameRequestModal(user);
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
                        gameStarter.StartGame(match, matchAccepted.MatchID);
                        mainMenu.gameObject.SetActive(false);
                        break;
                    case PushMessageType.NewMove:
                        string matchId = messageReceivedEvent.Message;
                        await gameStarter.OnNewMovePushMessageReceived(matchId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Debug.LogError($"{messageReceivedEvent.MessageType} is not a valid push message type");
            }
        }

        // Send a push message to the friend that opens their game request overlay.
        // The friend accepting the game will start the match.
        private static async Task SendGameRequest(string challengerName, string receiverPlayerId)
        {
            PushMessagesServiceBindings pushMessagesServiceModule = new();
            bool success = await pushMessagesServiceModule
                .ChallengeFriend(challengerName, receiverPlayerId);

            string log = success ? "Challenge Friend Success" : "Challenge Friend Failed";
            Debug.Log(log);
        }

        private static void OnRelationShipDeleted(IRelationshipDeletedEvent relationshipDeletedEvent, MainMenu mainMenu)
        {
            switch (relationshipDeletedEvent.Relationship.Type)
            {
                case RelationshipType.Friend:
                    throw new NotImplementedException();
                    break;
                case RelationshipType.FriendRequest:
                    mainMenu.HideFriendRequest();
                    break;
                case RelationshipType.Block:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void OnRelationShipAdded(IRelationshipAddedEvent relationshipAddedEvent, MainMenu mainMenu)
        {
            switch (relationshipAddedEvent.Relationship.Type)
            {
                case RelationshipType.Friend:
                    mainMenu.ShowFriend(relationshipAddedEvent.Relationship.Member);
                    break;
                case RelationshipType.FriendRequest:
                    mainMenu.ShowFriendRequest(relationshipAddedEvent.Relationship.Member);
                    break;
                case RelationshipType.Block:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}