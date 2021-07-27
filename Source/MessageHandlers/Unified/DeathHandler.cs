using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModThatIsNotMod;
using MultiplayerMod.Core;
using MultiplayerMod.MessageHandlers;
using MultiplayerMod.Networking;

namespace MultiplayerMod.MessageHandlers.Unified
{
    [MessageHandler(MessageType.Death, PeerType.Both)]
    class DeathHandler : MessageHandler
    {
        public override void HandleMessage(MessageType msgType, ITransportConnection connection, P2PMessage msg)
        {
            DeathMessage deathMessage = new DeathMessage(msg);

            GameObject ford = CustomItems.SpawnFromPool("fords name", deathMessage.position, deathMessage.rotation);
            ford.GetComponent<AIBrain>().health.TakeDamage(1, new Attack{ Damage = float.PositiveInfinity });

            if (peer.Type == PeerType.Server)
            {
                Server.Players.SendMessageToAllExcept(DeathMessage, SendReliability.Reliable, connection.ConnectedTo);
            }
        }
    }
}
