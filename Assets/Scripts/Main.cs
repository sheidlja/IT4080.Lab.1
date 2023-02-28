using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace it4080
{
    public class TestMain : NetworkBehaviour
    {
        private NetworkManager netMgr;

        public It4080.NetworkSettings netSettings;
        //public It4080.Chat chat;

        private Button _btnStartGame;
        public Button btnClient;
        public Button btnHost;
        public Button btnServer;

        void Start()
        {
            netSettings.startClient += NetSettingsOnStartClient;
            netSettings.startHost += NetSettingsOnStartHost;
            netSettings.startServer += NetSettingsOnStartServer;
            netSettings.setStatusText("Not Connected");

            //chat.sendMessage += ChatOnSendMessage;

            _btnStartGame = GameObject.Find("BtnStartGame").GetComponent<Button>();
            _btnStartGame.onClick.AddListener(BtnStartGameOnClick);
            _btnStartGame.gameObject.SetActive(false);
        }

        private void StartGame()
        {
            NetworkManager.SceneManager.LoadScene("Arena1",
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void BtnStartGameOnClick()
        {
            StartGame();
        }


       //private void ChatOnSendMessage(It4080.SendChatMessageServerRpc msg)
       // {
      //      chat.RequestSendMessageServerRpc(msg.message);
       // }

        private void setupTransport(IPAddress ip, ushort port)
        {
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            utp.ConnectionData.Address = ip.ToString();
            utp.ConnectionData.Port = port;
        }

        private void printIs(string msg)
        {
            Debug.Log($"[{msg}]  {MakeIsString()}");
        }

        private string MakeIsString()
        {
            return $"server:{IsServer} host:{IsHost} client:{IsClient} owner:{IsOwner}";
        }

        private string MakeWelcomeMessage(ulong clientId)
        {
            return $"Welcome to the server.  You are player {clientId}.  Have a good time.";
        }

        private void StartServer(IPAddress ip, ushort port)
        {
            Debug.Log($"Starting server {ip}:{port}");

            NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;

            netSettings.setStatusText("Starting Server");

            setupTransport(ip, port);
            NetworkManager.Singleton.StartServer();
            netSettings.hide();
        }

        private void StartHost(IPAddress ip, ushort port)
        {
            Debug.Log($"Starting host {ip}:{port}");

            NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;

            netSettings.setStatusText("Starting Host");

            setupTransport(ip, port);
            NetworkManager.Singleton.StartHost();
            netSettings.hide();
        }

        private void StartClient(IPAddress ip, ushort port)
        {
            Debug.Log($"Starting client {ip}:{port}");

            NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientOnClientDisconnect;

            setupTransport(ip, port);
            NetworkManager.Singleton.StartClient();

            netSettings.setStatusText("Connecting to host/server");
            netSettings.hide();
        }

        // ----------------------
        // Host Events
        // ----------------------
        private void HostOnServerStarted()
        {
            printIs("host/server started event");
            if (IsHost)
            {
                netSettings.setStatusText($"Host Running.  We are client {NetworkManager.Singleton.LocalClientId}.");
                //chat.enabled = true;
                //chat.enable(true);
            }
            else
            {
                netSettings.setStatusText("Server Running");
            }
            // Displays locally for the host/server only.
            //chat.SystemMessage("Server/Host Started");
        }

        private void HostOnClientConnected(ulong clientId)
        {
            // Tell everyone that a new client connected
            //chat.SendSystemMessageServerRpc($"Client {clientId} connected.");
            // Send the welcome message to the newly connected client only
            /*chat.SendSystemMessageServerRpc(
                MakeWelcomeMessage(clientId),
                clientId);*/
        }

        private void HostOnClientDisconnected(ulong clientId)
        {
            //chat.SendSystemMessageServerRpc($"Client {clientId} disconnected.");
        }

        // ----------------------
        // Client Events
        // ----------------------
        private void ClientOnClientConnected(ulong clientId)
        {
            netSettings.setStatusText($"Connected as {clientId}");
            //chat.enabled = true;
            //chat.enable(true);
        }

        private void ClientOnClientDisconnect(ulong clientId)
        {
            // Must manually create a system message in our chat control
            // here since we do not have a connection to the server.
            //chat.SystemMessage("Disconnected from Server.");
            netSettings.setStatusText("Connection Lost");
            netSettings.show();
            //chat.enable(false);
        }

        // ----------------------
        // netSettings Events
        // ----------------------
        private void NetSettingsOnStartClient(IPAddress ip, ushort port)
        {
            StartClient(ip, port);
        }

        private void NetSettingsOnStartHost(IPAddress ip, ushort port)
        {
            StartHost(ip, port);
        }

        private void NetSettingsOnStartServer(IPAddress ip, ushort port)
        {
            StartServer(ip, port);
        }
    }
}