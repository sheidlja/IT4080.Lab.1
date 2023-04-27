using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Playables;

public class ArenaManager : NetworkBehaviour
{
    public It4080.Player playerPrefab;

    private It4080.ScoreBoard scoreBoard;
    int currentSpawnTeam = 0;

    private void Start() {
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        scoreBoard = GameObject.Find("ScoreBoard").GetComponent<It4080.ScoreBoard>();
        Debug.Log($"Scoreboard = {scoreBoard}");

        if (IsServer) {
            SpawnAllPlayers();
        }
    }


    private void SpawnAllPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            SpawnPlayerForClient(clientId);
        }
    }


    private It4080.Player SpawnPlayerForClient(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.Log("Prefab is null, set it in the editor dummy...you made this you should know how to use it.");
        }
        Vector3 spawnPosition = new Vector3(-2.5f, 1f, -2.5f);

        if (currentSpawnTeam == 1)
        {
            spawnPosition.x = 2.5f;
            spawnPosition.z = 2.5f;
        }
        It4080.Player playerSpawn = Instantiate(
                                playerPrefab,
                                spawnPosition,
                                Quaternion.identity);
        playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        scoreBoard.AddPlayer(playerSpawn, $"Player {clientId}");

        playerSpawn.netTeamID.Value = currentSpawnTeam;

        if (currentSpawnTeam == 0)
        {
            currentSpawnTeam = 1;
        }
        else 
        {
            currentSpawnTeam = 0;
        }

        return playerSpawn;


    }
}
