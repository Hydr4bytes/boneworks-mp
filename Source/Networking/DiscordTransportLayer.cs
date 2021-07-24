using System;
using Discord;

namespace MultiplayerMod.Networking
{
    private static Discord.Discord discord;
    
    public class DiscordTransportConnection : ITransportConnection { 
        public ulong ConnectedTo { get; private set; }
        public bool IsConnected => IsValid;
        internal bool IsValid { get; private set; } = true;

        internal DiscordTransportConnection(ulong id, P2PMessage initialMessage) {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.ConnectNetwork(id);
            // Reliable on 0, unreliable on 1
            lobbyManager.OpenNetworkChannel(id, 0, true);
            lobbyManager.OpenNetworkChannel(id, 1, false);
            ConnectedTo = id;

            SendMessage(initialMessage, SendReliability.Reliable);
            MelonLogger.Msg($"Discord: Sent initial message to {id}");
        }

        public void Disconnect() {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.DisconnectLobby(lobbyManager.GetLobbyId(0));
            IsValid = false;
        }

        public void SendMessage(P2PMessage msg, SendReliability sendType) {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.SendNetworkMessage(lobbyManager.GetLobbyId(0), ConnectedTo, sendType, msg.GetBytes());
        }
    }

    public class DiscordTransportLayer : ITransportLayer {
        public event Action<ITransportConnection, ConnectionClosedReason> OnConnectionClosed;
        public event Action<ITransportConnection, P2PMessage> OnMessageReceived;
        private readonly Dictionary<ulong, DiscordTransportConnection> connections = new Dictionary<ulong, DiscordTransportConnection>();

        public ITransportConnection ConnectTo(ulong id, P2PMessage initialMessage) {
            if (connections.ContainsKey(id)) {
                if (connections[id].IsValid)
                    throw new ArgumentException("Already connected to " + id.ToString());
                else
                    connections.Remove(id);
            }

            MelonLogger.Msg($"Discord: Connecting to {id}");
            DiscordTransportConnection connection = new DiscordTransportConnection(id, initialMessage);
            connections.Add(id, connection);
            SteamNetworking.OnP2PSessionRequest = ClientOnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed = ClientOnP2PConnectionFailed;

            return connection;
        }

        public void StartListening() {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.OnNetworkMessage = (lobbyId, userId, channelId, data) => {
                OnMessageReceived.Invoke(connections[userId], new P2PMessage(data));
            };
        }

        public void StopListening() {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.OnNetworkMessage = null;
        }

        public void Update() {
            var lobbyManager = discord.GetLobbyManager();
            lobbyManager.FlushNetwork();
        }
    }
}