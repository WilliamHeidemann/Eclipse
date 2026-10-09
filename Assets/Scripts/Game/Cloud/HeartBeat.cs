using System;
using System.Threading;
using Game.Domain;
using UnityEngine;

namespace Game.Cloud
{
    public class HeartBeat 
    {
        private readonly string _matchId;
        private readonly PlayCoordinator _playCoordinator;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public HeartBeat(string matchId, PlayCoordinator playCoordinator)
        {
            _matchId = matchId;
            _playCoordinator = playCoordinator;
        }

        public async Awaitable RunAsync()
        {
            while (true)
            {
                try
                {
                    await Awaitable.WaitForSecondsAsync((float)_interval.TotalSeconds);
                    await _playCoordinator.CheckIfOpponentPlayed(_matchId);
                    Debug.Log("Heartbeat check completed");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Heartbeat check failed: {e.Message}");
                }
            }
        }
    }
}