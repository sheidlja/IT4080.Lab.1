using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class TestMain : NetworkBehaviour {
    private NetworkManager netMgr;

    public It4080.NetworkSettings netSettings;
    public ChatServer chatServer;
    public It4080.ChatUi chat;

    private Button _btnStart;


    void Start() {
        netSettings.startClient += NetSettingsOnStartClient;
        netSettings.startHost += NetSettingsOnStartHost;
        netSettings.startServer += NetSettingsOnStartServer;
        netSettings.setStatusText("Not Connected");
        netSettings.ShowServer = true;

        if (chat != null) {
            chat.OnTextSubmitted += ChatOnSendMessage;
        }

        _btnStart = GameObject.Find("BtnStartGame").GetComponent<Button>();
        _btnStart.onClick.AddListener(BtnStartGameOnClick);
        _btnStart.gameObject.SetActive(false);
    }


    // -----------------------
    // Private
    // -----------------------

    private void SetupTransport(IPAddress ip, ushort port) {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;
    }


    private void GotoLobby() {
        NetworkManager.SceneManager.LoadScene("TestLobby",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }


    private void StartGame() {
        NetworkManager.SceneManager.LoadScene("Arena1",
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }


    private void PrintIs(string msg) {
        Debug.Log($"[{msg}]  {MakeIsString()}");
    }


    private string MakeIsString() {
        return $"server:{IsServer} host:{IsHost} client:{IsClient} owner:{IsOwner}";
    }


    private string MakeWelcomeMessage(ulong clientId) {
        return $"Welcome to the server.  You are player {clientId}.  Have a good time.";
    }


    private void StartServer(IPAddress ip, ushort port) {
        Debug.Log($"Starting server {ip}:{port}");

        NetworkManager.Singleton.OnServerStarted += ServerOnServerStarted;

        netSettings.setStatusText("Starting Server");

        SetupTransport(ip, port);
        NetworkManager.Singleton.StartServer();
        netSettings.hide();

        _btnStart.gameObject.SetActive(true);
    }


    private void StartHost(IPAddress ip, ushort port) {
        Debug.Log($"Starting host {ip}:{port}");

        NetworkManager.Singleton.OnServerStarted += ServerOnServerStarted;
        netSettings.setStatusText("Starting Host");

        SetupTransport(ip, port);
        NetworkManager.Singleton.StartHost();
        netSettings.hide();

        _btnStart.gameObject.SetActive(true);
    }


    private void StartClient(IPAddress ip, ushort port) {
        Debug.Log($"Starting client {ip}:{port}");

        SetupTransport(ip, port);
        NetworkManager.Singleton.StartClient();

        netSettings.setStatusText("Connecting to host/server");
        netSettings.hide();
    }


    // -----------------------
    // General Events
    // -----------------------
    private void BtnStartGameOnClick() {
        StartGame();
    }


    private void ChatOnSendMessage(string message) {
        chatServer.RequestSendMessageServerRpc(message);
    }


    // ----------------------
    // Server/Host Events
    // ----------------------
    private void ServerOnServerStarted() {
        PrintIs("host/server started event");
        if (IsHost) {
            netSettings.setStatusText($"Host Running.  We are client {NetworkManager.Singleton.LocalClientId}.");
            chat.enabled = true;
            chat.enable(true);
        } else {
            netSettings.setStatusText("Server Running");
        }
        // Displays locally for the host/server only.
        chat.DisplayText("Server/Host Started");
        GotoLobby();
    }




    // ----------------------
    // netSettings Events
    // ----------------------
    private void NetSettingsOnStartClient(IPAddress ip, ushort port) {
        StartClient(ip, port);
    }


    private void NetSettingsOnStartHost(IPAddress ip, ushort port) {
        StartHost(ip, port);
    }


    private void NetSettingsOnStartServer(IPAddress ip, ushort port) {
        StartServer(ip, port);
    }
}
