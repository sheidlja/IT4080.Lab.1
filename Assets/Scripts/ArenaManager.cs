using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace It4080
{
    public class ArenaManager : NetworkBehaviour
    {
        public Player playerPrefab;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                SpawnAllPlayers();
            }
        }

        private Player SpawnPlayerForClient(ulong clientId)
        {
            Vector3 spawnPosition = new Vector3(0, 1, clientId * 5);
            Player playerSpawn = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            return playerSpawn;
        }

        private void SpawnAllPlayers()
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                SpawnPlayerForClient(clientId);
            }
        }
       
    }
}
