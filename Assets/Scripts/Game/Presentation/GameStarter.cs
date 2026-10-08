using System;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Players;
using Game.Domain.Players.Bot_Strategies;
using Game.Domain.Server;
using Game.Presentation.AnimationSystems;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;
using UtilityToolkit.Monads;

namespace Game.Presentation
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private AnimationOrchestrator _animationOrchestrator;

        private PlayCoordinator _playCoordinator;
        private Option<string> _matchId;
        private HeartBeat _heartBeat;

        private void BindToPresentation(PlayCoordinator playCoordinator)
        {
            _animationOrchestrator.BindAnimations(playCoordinator);
            playCoordinator.RaiseDrawHandEvent();
            _boardPresenter.OnPositionClicked += HandlePositionClicked;
        }
        
        public void StartLocalGame()
        {
            _playCoordinator = CreateLocalPlayCoordinator();
            BindToPresentation(_playCoordinator);
            return;
            
            static PlayCoordinator CreateLocalPlayCoordinator()
            {
                GameState gameState = GameState.CreateInitial();

                ClientGameState clientGameState = gameState.ToClientGameState(gameState.ToPlay);

                LocalGameServer playerGameServer = CreateLocalGameServer(gameState);
                
                playerGameServer.OnScored += (score, team) => Debug.Log($"Scored {score} for team {team}");

                return new PlayCoordinator(playerGameServer, clientGameState);
            
                static LocalGameServer CreateLocalGameServer(GameState gameState)
                {
                    LocalGameState localGameState = new(gameState);
                    LocalGameServer playerGameServer = new(localGameState);
                    LocalGameServer botGameServer = new(localGameState);
                    playerGameServer.OtherPlayerServer = botGameServer;
                    botGameServer.OtherPlayerServer = playerGameServer;
                    new Bot(botGameServer, new CenterBrain());

                    return playerGameServer;
                }
            }
        }
        
        public async Task<Match> CreateMatch(string opponentId)
        {
            GameLogicServiceBindings gameLogicServiceBindings = new();
            var matchDto = await gameLogicServiceBindings.CreateMatch(opponentId);
            Match match = matchDto.ToModel();
            return match;
        }

        public void StartGame(Match match, string opponentId)
        {
            _playCoordinator = CreatePlayCoordinator(match);
            _playCoordinator.OnValidMoveRequest += async move => await NotifyOpponent(opponentId);
            _matchId = Option<string>.Some(match.Id);
            BindToPresentation(_playCoordinator);
            _heartBeat = new HeartBeat(match.Id, _playCoordinator);
            _heartBeat.RunAsync().Forget();
        }

        private async Task NotifyOpponent(string opponentId)
        {
            if (!_matchId.IsSome(out string matchId))
            {
                Debug.LogError("MatchId does not exist. Opponent was not notified.");
                return;
            }
            
            PushMessagesServiceBindings pushMessagesServiceBindings = new();
            bool success = await pushMessagesServiceBindings.NotifyOpponentOfMove(matchId, opponentId);
            if (success)
            {
                Debug.Log($"Opponent {opponentId} was successfully notified of move.");
            }
            else
            {
                Debug.LogError($"Opponent {opponentId} was not notified of move.");
            }
        }

        public async Task OnNewMovePushMessageReceived(string matchId)
        {
            if (_playCoordinator != null)
            {
                await _playCoordinator.CheckIfOpponentPlayed(matchId);
            }
        }

        private PlayCoordinator CreatePlayCoordinator(Match match)
        {
            CloudGameServer playerGameServer = CreateCloudGameServer(match.Id);
            
            playerGameServer.OnScored += (score, team) => Debug.Log($"Scored {score} for team {team}");
            
            return new PlayCoordinator(playerGameServer, match.ClientGameState);
            
            CloudGameServer CreateCloudGameServer(string matchId)
            {
                GameLogicServiceBindings gameLogicService = new();
                return new CloudGameServer(gameLogicService, matchId);
            }
        }

        private async void HandlePositionClicked(Position position)
        {
            try
            {
                await _playCoordinator.PositionClicked(position);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task OnDestroy()
        {
            if (_matchId.IsSome(out string matchId))
            {
                GameLogicServiceBindings gameLogicServiceBindings = new();
                bool success = await gameLogicServiceBindings.DeleteMatch(matchId);
                Debug.Log(success);
            }
        }
    }
}