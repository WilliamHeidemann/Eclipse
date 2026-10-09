using System;
using System.Net;
using System.Threading.Tasks;
using Game.Domain.Models;
using Game.Domain.Models.Dto;
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
            MoveValidator.MoveResult.Success(var nextGameState, var deltaScore) => await
                SuccessMoveRequestResult(context, matchId, nextGameState, move.Team, deltaScore),
            MoveValidator.MoveResult.Invalid => new MoveRequestResult { WasValid = false, },
            MoveValidator.MoveResult.OutOfSync => new MoveRequestResult { WasValid = false, },
            _ => throw new ArgumentOutOfRangeException()
        };

        return moveRequestResult;
    }

    private async Task<MoveRequestResult> SuccessMoveRequestResult(IExecutionContext context, string matchId,
        GameState next, Team team, int deltaScore)
    {
        ApiResponse<SetItemResponse> response = await SetGameState(context, matchId, next);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            // maybe start returning an enum instead? Or an algebraic data type so the card can be return?
            return new MoveRequestResult { WasValid = false };
        }

        return new MoveRequestResult { WasValid = true, UpdatedGameState = next.ToClientGameState(team), DeltaScore = deltaScore };
    }


    [CloudCodeFunction]
    public async Task<ClientGameState> GetClientGameState(IExecutionContext context, string matchId)
    {
        GameState gameState = await GetGameState(context, matchId);
        Team team = await GetMyTeam(context, matchId);
        return gameState.ToClientGameState(team);
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
        var result = await gameApiClient.CloudSaveData.DeletePrivateCustomItemsAsync(context,
            context.ServiceToken, context.ProjectId, matchId);

        return result.StatusCode == HttpStatusCode.OK;
    }
}