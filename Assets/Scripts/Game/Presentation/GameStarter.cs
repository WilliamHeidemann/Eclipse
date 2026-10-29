using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Game.Domain.Players;
using Game.Domain.Players.Bot_Strategies;
using Game.Domain.Server;
using Game.Presentation.AnimationSystems;
using Game.Presentation.UI;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;
using UtilityToolkit.Monads;

namespace Game.Presentation
{
    public class GameStarter : MonoBehaviour
    {
        [SerializeField] private BoardPresenter _boardPresenter;
        [SerializeField] private AnimationOrchestrator _animationOrchestrator;
        [SerializeField] private StartingConfiguration _startingConfiguration;
        
        private PlayCoordinator _playCoordinator;
        private Option<string> _matchId;
        private HeartBeat _heartBeat;

        private void BindToPresentation(PlayCoordinator playCoordinator)
        {
            _animationOrchestrator.BindAnimations(playCoordinator);
            playCoordinator.RaiseStartGameEvents();
            _boardPresenter.OnPositionClicked += HandlePositionClicked;
        }

        public void StartLocalGame(BotDifficulty difficulty)
        {
            _animationOrchestrator.ShowStaticGameElements();
            
            GameState gameState = _startingConfiguration.UseDevelopmentStartingState
                ? _startingConfiguration.DevelopmentGameState()
                : GameState.CreateInitial();
            
            if (_startingConfiguration.UseDevelopmentStartingState)
            {
                foreach (Move preMove in gameState.Moves)
                {
                    _animationOrchestrator.PlayDiscardAndPinAnimation(preMove);
                }
            }
            
            _playCoordinator = CreateLocalPlayCoordinator(gameState, difficulty);

            BindToPresentation(_playCoordinator);
        }
        
        public PlayCoordinator CreateLocalPlayCoordinator(GameState gameState, BotDifficulty difficulty)
        {
            ClientGameState clientGameState = gameState.ToClientGameState(gameState.ToPlay);

            LocalGameServer playerGameServer = CreateLocalGameServer(gameState, difficulty);

            return new PlayCoordinator(playerGameServer, clientGameState);
        }

        private LocalGameServer CreateLocalGameServer(GameState gameState, BotDifficulty difficulty)
        {
            LocalGameState localGameState = new(gameState);
            LocalGameServer playerGameServer = new(localGameState);
            LocalGameServer botGameServer = new(localGameState);
            playerGameServer.OtherPlayerServer = botGameServer;
            botGameServer.OtherPlayerServer = playerGameServer;
            IBrain botBrain = difficulty switch
            {
                BotDifficulty.Easy => new RandomBrain(),
                BotDifficulty.Hard => new CenterBrain(),
                BotDifficulty.Extreme => new CenterBrain(),
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
            Bot bot = new(botGameServer, botBrain);
            bot.OnStartThinking += () => _animationOrchestrator.QueueDelay(1f);

            return playerGameServer;
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
            _animationOrchestrator.ShowStaticGameElements();
            _playCoordinator = CreatePlayCoordinator(match);
            _playCoordinator.OnValidMoveRequest += async _ => await NotifyOpponent(opponentId);
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

        public void EndGame()
        {
            _animationOrchestrator.HideGameElements();
        }
    }
}