using System;
using System.Threading.Tasks;
using Game.Cloud;
using Game.Domain.Models;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

namespace Game.Presentation
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenu _mainMenu;
        [SerializeField] private GameStarter _gameStarter;
        [SerializeField] private StartingConfiguration _startingConfiguration;
        
        private async Task Awake()
        {
            try
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                
                _mainMenu.gameObject.SetActive(_startingConfiguration.StartIn == StartIn.MainMenu);
                
                switch (_startingConfiguration.Mode)
                {
                    case Mode.Online:
                        await CloudHandler.Initialize(_mainMenu, _gameStarter);
                        // var matchId = await _gameStarter.StartOnlineGame("123");
                        // GameLogicServiceBindings g = new();
                        // var t = await g.GetClientGameState(matchId);
                        // Debug.Log($"{t.ToModel()}");
                        break;
                    
                    case Mode.Local when _startingConfiguration.StartIn == StartIn.Game:
                        _gameStarter.StartLocalGame();
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