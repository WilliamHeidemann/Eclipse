using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
using Game.Domain.Models.Organisms;
using Game.Domain.Models.State;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace Cloud_Code_Module_Reference;

public class GameLogicService(IGameApiClient gameApiClient)
{
    [Serializable]
    private record Teams(string Red, string Yellow);

    [CloudCodeFunction]
    public async Task<Match> CreateMatch(IExecutionContext context, string opponentId)
    {
        string matchId = Guid.NewGuid().ToString();

        if (context.PlayerId == null) throw new NullReferenceException("context.PlayerId is null");

        Teams teams = AssignTeams(context.PlayerId, opponentId);
        ApiResponse<SetItemResponse> setTeamsResponse = await SetTeams(context, matchId, teams);

        GameState gameState = GameState.CreateInitial();
        ApiResponse<SetItemResponse> setMatchResponse = await SetGameState(context, matchId, gameState);

        await AddActiveMatch(context, context.PlayerId, matchId);
        await AddActiveMatch(context, opponentId, matchId);

        Team myTeam = GetTeam(context.PlayerId, teams);
        ClientGameState clientGameState = gameState.ToClientGameState(myTeam);

        return new Match
        {
            Id = matchId,
            ClientGameState = clientGameState
        };
    }

    private Teams AssignTeams(string player1, string player2)
    {
        return Random.Shared.NextDouble() < 0.5
            ? new Teams(player1, player2)
            : new Teams(player2, player1);
    }

    private async Task<ApiResponse<SetItemResponse>> SetTeams(IExecutionContext context, string matchId, Teams teams)
    {
        SetItemBody setGameData = new("teams", teams);

        return await gameApiClient.CloudSaveData.SetPrivateCustomItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            matchId,
            setGameData);
    }

    private async Task<ApiResponse<SetItemResponse>> SetGameState(IExecutionContext context, string matchId,
        GameState gameState)
    {
        SetItemBody setGameData = new("gameState", gameState);

        return await gameApiClient.CloudSaveData.SetPrivateCustomItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            matchId,
            setGameData);
    }

    [CloudCodeFunction]
    public async Task<MoveRequestResult> Request(IExecutionContext context, Move move, string matchId)
    {
        GameState currentGameState = await GetGameState(context, matchId);

        MoveRequestResult moveRequestResult = MoveValidator.PlayMove(currentGameState, move) switch
        {
            MoveResult.Success success => await
                SuccessMoveRequestResult(context, matchId, success.UpdatedState, move.Team),
            MoveResult.Invalid => new MoveRequestResult { WasValid = false, },
            _ => throw new ArgumentOutOfRangeException()
        };

        return moveRequestResult;
    }

    private async Task<MoveRequestResult> SuccessMoveRequestResult(IExecutionContext context, string matchId,
        GameState next, Team team)
    {
        ApiResponse<SetItemResponse> response = await SetGameState(context, matchId, next);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            // maybe start returning an enum instead? Or an algebraic data type so the card can be return?
            return new MoveRequestResult { WasValid = false };
        }

        // The winner has seen the result. The opponent's entry and the match data are cleared once they fetch it.
        if (HasWon(next, team) && context.PlayerId != null)
        {
            await RemoveActiveMatch(context, context.PlayerId, matchId);
        }

        return new MoveRequestResult { WasValid = true, UpdatedGameState = next.ToClientGameState(team) };
    }


    [CloudCodeFunction]
    public async Task<ClientGameState> GetClientGameState(IExecutionContext context, string matchId)
    {
        GameState gameState = await GetGameState(context, matchId);
        Team team = await GetMyTeam(context, matchId);
        ClientGameState clientGameState = gameState.ToClientGameState(team);

        if (HasWon(gameState, team.Opposing()))
        {
            await DeleteMatchData(context, matchId);
        }

        return clientGameState;
    }

    [CloudCodeFunction]
    public async Task<List<string>> GetActiveMatchIds(IExecutionContext context)
    {
        if (context.PlayerId == null) throw new NullReferenceException("context.PlayerId is null");

        List<string> matchIds = [];
        string? after = null;
        do
        {
            ApiResponse<GetKeysResponse> response = await gameApiClient.CloudSaveData.GetPrivateCustomKeysAsync(
                context,
                context.ServiceToken,
                context.ProjectId,
                PlayerMatchesId(context.PlayerId),
                after);

            matchIds.AddRange(response.Data.Results.Select(key => key.Key));
            after = response.Data.Links?.Next;
        } while (!string.IsNullOrEmpty(after));

        return matchIds;
    }

    private static bool HasWon(GameState gameState, Team team)
    {
        return gameState.Sequences.Count(sequence => sequence.Team == team) >= 2;
    }

    private static string PlayerMatchesId(string playerId) => $"player-matches-{playerId}";

    private async Task AddActiveMatch(IExecutionContext context, string playerId, string matchId)
    {
        await gameApiClient.CloudSaveData.SetPrivateCustomItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            PlayerMatchesId(playerId),
            new SetItemBody(matchId, true));
    }

    private async Task RemoveActiveMatch(IExecutionContext context, string playerId, string matchId)
    {
        await gameApiClient.CloudSaveData.DeletePrivateCustomItemAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            PlayerMatchesId(playerId),
            matchId);
    }

    private async Task DeleteMatchData(IExecutionContext context, string matchId)
    {
        ApiResponse<GetItemsResponse> response = await gameApiClient.CloudSaveData.GetPrivateCustomItemsAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            matchId,
            ["teams"]);

        string rawJson = JsonConvert.SerializeObject(response.Data.Results[0].Value);
        Teams teams = JsonConvert.DeserializeObject<Teams>(rawJson)
                      ?? throw new JsonException("Could not deserialize teams.");

        await RemoveActiveMatch(context, teams.Red, matchId);
        await RemoveActiveMatch(context, teams.Yellow, matchId);
        await gameApiClient.CloudSaveData.DeletePrivateCustomItemsAsync(
            context, context.ServiceToken, context.ProjectId, matchId);
    }

    private async Task<Team> GetMyTeam(IExecutionContext context, string matchId)
    {
        if (context.PlayerId == null) throw new NullReferenceException("context.PlayerId is null");
        
        ApiResponse<GetItemsResponse> response = await gameApiClient.CloudSaveData.GetPrivateCustomItemsAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            matchId,
            ["teams"]);

        Item item = response.Data.Results[0];
        string rawJson = JsonConvert.SerializeObject(item.Value);
        Teams teams = JsonConvert.DeserializeObject<Teams>(rawJson)
                      ?? throw new JsonException("Could not deserialize teams.");

        return GetTeam(context.PlayerId, teams);
    }

    private static Team GetTeam(string playerId, Teams teams)
    {
        return playerId switch
        {
            _ when playerId == teams.Red => Team.Red,
            _ when playerId == teams.Yellow => Team.Yellow,
            _ => throw new InvalidOperationException("Player is not on any team.")
        };
    }

    private async Task<GameState> GetGameState(IExecutionContext context, string matchId)
    {
        ApiResponse<GetItemsResponse> response = await gameApiClient.CloudSaveData.GetPrivateCustomItemsAsync(
            context,
            context.ServiceToken,
            context.ProjectId,
            matchId,
            ["gameState"]);

        Item item = response.Data.Results[0];
        string rawJson = JsonConvert.SerializeObject(item.Value);
        GameState gameState = JsonConvert.DeserializeObject<GameState>(rawJson)
                              ?? throw new JsonException("Could not deserialize game state.");

        return gameState;
    }

    [CloudCodeFunction]
    public async Task<bool> DeleteMatch(IExecutionContext context, string matchId)
    {
        await GetMyTeam(context, matchId); // Throws if the caller is not part of the match.
        await DeleteMatchData(context, matchId);
        return true;
    }
}
