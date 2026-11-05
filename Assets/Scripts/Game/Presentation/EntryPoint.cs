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

        private CloudHandler _cloudHandler;
        
        private async Task Start()
        {
            try
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                
                _cloudHandler = new CloudHandler(_uiManager, _gameStarter);
                await _cloudHandler.Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
    }
}