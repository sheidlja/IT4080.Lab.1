/*using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace It4080
{

    public class Chat : NetworkBehaviour
    {
        const ulong SYSTEM_ID = 999999999;
        public TMPro.TMP_Text txtChatLog;
        public Button btnSend;
        public TMPro.TMP_InputField inputMessage;

        ulong[] singleClientId = new ulong[1];

        public void Start()
        {
            btnSend.onClick.AddListener(ClientOnSendClicked);
            txtInput.onSubmit.AddListener(ClientOnInputSubmit);
        }

        public override void OnNetworkSpawn()
        {
            txtChatLog.text = "-- Howdy --";

            if (IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnect;
                DisplayMessageLocally("You are the Host!", SYSTEM_ID);
            }
            else
            {
                DisplayMessageLocally($"You are Player #{NetworkManager.Singleton.LocalClientId}!", SYSTEM_ID);
            }
        }

        private void SendUIMessage()
        {
            string msg = inputMessage.text;
            inputMessage.text = "";
            SendChatMessageServerRpc(msg);
        }

        private void SendDirectMessage(string message, ulong from, ulong to)
        {
            ClientRpcParams rpcParams = default;
            rpcParams.Send.TargetClientIds = singleClientId;

            singleClientId[0] = from;
            SendChatMessageClientRpc($"<whisper> {message}", from, rpcParams);

            singleClientId[0] = to;
            SendChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
        }

        //-----------------------------------------------------
        //Events
        //-----------------------------------------------------
        private void HostOnClientConnected(ulong clientId)
        {
            SendChatMessageClientRpc($"Client {clientId} connected", SYSTEM_ID);
        }

        private void HostOnClientDisconnected(ulong clientId)
        {
            SendChatMessageClientRpc($"Client {clientId} disconnected", SYSTEM_ID);
        }

        public void ClientOnSendClicked()
        {
            SendUIMessage();
        }

        public void ClientOnInputSubmit(string text)
        {
            SendUIMessage();
        }

        //-----------------------------------------------------
        //RPC
        //-----------------------------------------------------

        [ClientRpc]
        public void SendChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
        {
            DisplayMessageLocally(message, from);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
        {
            Debug.Log($"Host got message: {message}");

            if (message.StartsWith("@"))
            {
                string[] parts = message.Splt(" ");
                string clientIdStr = parts[0].Replace("@", "");
                ulong toClientId = ulong.Parse(clientIdStr);

                SendDirectMessage(message, serverRpcParams.Recieve.SenderClientId, toClientId);
            }
            else
            {
                SendChatMessageClientRpc(message, serverRpcParams.Recieve.SenderClientId);
            }
        }

        public void DisplayMessageLocally(string message, ulong from)
        {
            Debug.Log(message);
            string who;

            if (from = NetworkManager.Singleton.LocalClientId)
            {
                who = "you";
            }
            else if (from = SYSTEM_ID)
            {
                who = "system";
            }
            else if (from = NetworkManager.Singleton.LocalHostId)
            {
                who = "Host";
            }
            else
            {
                who = $"Player #{from}";
            }
            string newMessage = $"[{who}]:  {message}";
            txtChatLog.text += newMessage;
        }
    }
}*/