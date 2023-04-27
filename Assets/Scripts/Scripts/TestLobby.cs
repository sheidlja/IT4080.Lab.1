using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using It4080;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TestLobby : NetworkBehaviour {
    public It4080.ConnectedPlayers connectedPlayers;
    public Button btnStart;
    public TMPro.TMP_Text txtKicked;

    void Start() {
        initialClear();
        txtKicked.gameObject.SetActive(false);
    }


    public override void OnNetworkSpawn() {
        initialClear();

        // Clients watch the allPlayers NetworkList for changes and update their
        // display appropriately.  The Server does not have to explicitly call
        // a client rpc to notify the clients of changes to the list of connected
        // players.
        if (IsClient) {
            NetworkHandler.Singleton.allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);

            if (!IsHost) {
                NetworkManager.OnClientDisconnectCallback += ClientOnDisconnect;
            }
        }

        if (IsServer) {
            NetworkHandler.Singleton.allPlayers.OnListChanged += LogNetworkListEvent;
            btnStart.onClick.AddListener(OnBtnStartClicked);
        }

        btnStart.gameObject.SetActive(IsHost);
    }


    private void ClientOnDisconnect(ulong clientId) {
        txtKicked.gameObject.SetActive(true);
        connectedPlayers.gameObject.SetActive(false);
    }


    private void ClientOnAllPlayersChanged(NetworkListEvent<It4080.PlayerData> changeEvent) {
        PopulateConnectedPlayersUsingPlayerDataList(
            NetworkHandler.Singleton.allPlayers);

        if (IsHost) {
            EnableStartIfAllReady();
        }
    }


    private void LogNetworkListEvent(NetworkListEvent<It4080.PlayerData> changeEvent)
    {
        Debug.Log($"Player data changed:");
        Debug.Log($"    Change Type:  {changeEvent.Type}");
        Debug.Log($"    Value:        {changeEvent.Value}");
        Debug.Log($"        {changeEvent.Value.clientId}");
        Debug.Log($"        {changeEvent.Value.isReady}");
        Debug.Log($"    Prev Value:   {changeEvent.PreviousValue}");
        Debug.Log($"        {changeEvent.PreviousValue.clientId}");
        Debug.Log($"        {changeEvent.PreviousValue.isReady}");
    }


    private bool hasClearedInitially = false;
    private void initialClear() {
        if (!hasClearedInitially) {
            connectedPlayers.Clear();
            hasClearedInitially = true;
        }
    }


    private void EnableStartIfAllReady() {
        int readyCount = 0;
        foreach (It4080.PlayerData readyInfo in NetworkHandler.Singleton.allPlayers) {
            if (readyInfo.isReady) {
                readyCount += 1;
            }
        }

        btnStart.enabled = readyCount == NetworkHandler.Singleton.allPlayers.Count;
        if (btnStart.enabled) {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game";
        } else {
            btnStart.GetComponentInChildren<TextMeshProUGUI>().text = "<Waiting for Ready>";
        }
    }


    private PlayerCard AddPlayerCard(ulong clientId) {
        It4080.PlayerCard newCard = connectedPlayers.AddPlayer("temp", clientId);
        string you = "";
        string what = "";

        newCard.ShowKick(IsServer);
        if (IsServer) {
            newCard.KickPlayer += ServerOnKickButtonPressed;
        }

        if (clientId == NetworkManager.LocalClientId) {
            you = "(you)";
            newCard.ShowReady(true);
            newCard.ReadyToggled += ClientOnMyReadyToggled;
        } else {
            you = "";
            newCard.ShowReady(false);
        }

        if (clientId == NetworkManager.ServerClientId) {
            what = "Host";
            newCard.ShowKick(false);
            newCard.ShowReady(false);
        } else {
            what = "Player";
        }

        newCard.SetPlayerName($"{what} {clientId}{you}");
        return newCard;
    }


    private void ServerOnKickButtonPressed(ulong clientId) {
        Debug.Log($"Kicking {clientId}");
        NetworkManager.DisconnectClient(clientId);
        NetworkHandler.Singleton.RemovePlayerFromList(clientId);
    }


    private void OnBtnStartClicked() {
        StartGame();
    }


    private void ClientOnMyReadyToggled(bool isReady) {
        RequestSetReadyServerRpc(isReady);
    }


    private void PopulateConnectedPlayersUsingPlayerDataList(NetworkList<It4080.PlayerData> players) {
        connectedPlayers.Clear();

        foreach(It4080.PlayerData p in players) {
            var card = AddPlayerCard(p.clientId);
            card.SetReady(p.isReady);
            string status = "Not Ready";
            if (p.isReady) {
                status = "READY!!";
            }
            card.SetStatus(status);
        }
    }


    // ---------------------
    // Public
    // ---------------------
    private void StartGame() {
        NetworkManager.SceneManager.LoadScene("Arena1",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestSetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default) {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int playerIndex = NetworkHandler.Singleton.FindPlayerIndex(clientId);
        It4080.PlayerData info = NetworkHandler.Singleton.allPlayers[playerIndex];

        info.isReady = isReady;
        NetworkHandler.Singleton.allPlayers[playerIndex] = info;
    }
}
