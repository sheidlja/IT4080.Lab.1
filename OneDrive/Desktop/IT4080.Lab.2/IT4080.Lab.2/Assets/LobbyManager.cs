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
            }

            base.OnNetworkSpawn();

            if (IsClient)
            {
                allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            }
            txtPlayerNumber.text = $"Player #{NetworkManager.LocalClientId}";
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
            AddPlayerToList(clientId);
            RefreshPlayerCards();
        }
       
        private void AddPlayerToList(ulong clientId)
        {
            allPlayers.Add(new PlayerData(clientId, Color.red));
        }

        private void AddPlayerCard (PlayerData info)
        {
            PlayerCard newCard = Instantiate(playerCard);
            newCard.transform.SetParent(playerScrollContent.transform, false);
            newCard.SetPlayerName($"Player {info.clientId.ToString()}");
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
    }
}
