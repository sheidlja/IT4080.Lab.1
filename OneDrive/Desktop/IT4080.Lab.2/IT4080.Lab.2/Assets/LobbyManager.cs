using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace It4080
{
    public class LobbyManager : NetworkBehaviour
    {
        public GameObject playerScrollContent;
        public TMPro.TMP_Text txtPlayerNumber;
        public Button btnStart;
        public Button btnReady;

        //public Player Player;
        public PlayerCard playerCard;

        private NetworkList<PlayerData> allPlayers = new NetworkList<PlayerData>();
        private List<PlayerCard> playerCards = new List<PlayerCard>();

        public void Start()
        {
            if (IsHost)
            {
                AddPlayerToList(NetworkManager.LocalClientId);
                RefreshPlayerCards();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
                //NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
                btnReady.gameObject.SetActive(false);
            }

            base.OnNetworkSpawn();

            if (IsClient && !IsHost)
            {
                allPlayers.OnListChanged += ClientOnAllPlayersChanged;
                btnStart.gameObject.SetActive(false);
            }
            txtPlayerNumber.text = $"Player #{NetworkManager.LocalClientId}";
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
        {
            if (IsHost)
            {
                return;
            }
            ulong clientId = rpcParams.Receive.SenderClientId;
            int playerIndex = NetworkHandler.Singleton.FindPlayerIndex(clientId);
            It4080.PlayerData info = NetworkHandler.Singleton.allPlayers[playerIndex];
            info.isReady = isReady;
            NetworkHandler.Singleton.allPlayers[playerIndex] = info;
            info = allPlayers[playerIndex];

            int readyCount = 0;
            foreach (PlayerData readyData in allPlayers)
            {
                if (readyData.isReady)
                {  
                    readyCount += 1;
                }
            }

            btnStart.enabled = readyCount == allPlayers.Count - 1;
        }

        //----------------------------------------
        // EVENTS
        //----------------------------------------
        private void ClientOnAllPlayersChanged(NetworkListEvent<PlayerData> changeEvent)
        {
            RefreshPlayerCards();
        }

        private void HostOnClientConnected(ulong clientId)
        {
            btnStart.enabled = false;
            AddPlayerToList(clientId);
            RefreshPlayerCards();
        }
       
        private void AddPlayerToList(ulong clientId)
        {
            allPlayers.Add(new PlayerData(clientId, Color.red, false));
        }

        private void AddPlayerCard (PlayerData info)
        {
            PlayerCard newCard = Instantiate(playerCard);
            newCard.transform.SetParent(playerScrollContent.transform, false);
            newCard.SetPlayerName($"Player {info.clientId.ToString()}");
            newCard.SetReady(info.isReady);
            playerCards.Add(newCard);
        }

        private void RefreshPlayerCards()
        {
            foreach (PlayerCard panel in playerCards)
            {
                Destroy(panel.gameObject);
            }
            playerCards.Clear();

            foreach (PlayerData pi in allPlayers)
            {
                AddPlayerCard(pi); 
            }
        }

        private int FindPlayerIndex(ulong clientId)
        {
            var idx = 0;
            var found = false;

            while (idx < allPlayers.Count && !found)
            {
                if (allPlayers[idx].clientId == clientId)
                {
                    found = true;
                }
                else
                {
                    idx += 1;
                }
            }

            if(!found)
            {
                idx = -1;
            }

            return idx;
        }

        /*private void ClientOnReadyClicked()
        {
            RequestSetReadyServerRpc(info.isReady);
        }*/
    }
}
