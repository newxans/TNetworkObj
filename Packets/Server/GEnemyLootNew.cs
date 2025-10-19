using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyLootNew : ServerPacket
    {
        public uint item_type;

        public Vector3 m_Position;

        public uint id;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(item_type);
            m_Position.ToWriter(p);
            p.wuint(id);

            return Packet.Pack(Protocols.GC_ENEMY_LOOT_NEW, p);
        }
    }
}