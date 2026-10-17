using System;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain.Models;
using Game.Presentation.UI;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

namespace Game.Presentation
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private GameStarter _gameStarter;
        [SerializeField] private StartingConfiguration _startingConfiguration;

        private CloudHandler2 _cloudHandler;
        
        private async Task Awake()
        {
            try
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                
                switch (_startingConfiguration.Mode)
                {
                    case Mode.Online:
                        _cloudHandler = new CloudHandler2(_uiManager, _gameStarter);
                        await _cloudHandler.Initialize();
                        // var matchId = await _gameStarter.StartOnlineGame("123");
                        // GameLogicServiceBindings g = new();
                        // var t = await g.GetClientGameState(matchId);
                        // Debug.Log($"{t.ToModel()}");
                        break;
                    
                    case Mode.Local when _startingConfiguration.StartIn == StartIn.Game:
                        _gameStarter.StartLocalGame(BotDifficulty.Hard);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }
}