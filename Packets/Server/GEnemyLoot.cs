using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyLoot : ServerPacket
    {
        public uint item_type;

        public Vector3 m_Position;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(item_type);
            m_Position.ToWriter(p);

            return Packet.Pack(Protocols.GC_ENEMY_LOOT, p);
        }
    }
}