using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyDead : ServerPacket
    {
        public uint m_iResult;

        public uint m_enemy_type;

        public uint bElite;

        public uint weapon_type;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iResult);
            p.wuint(m_enemy_type);
            p.wuint(bElite);
            p.wuint(weapon_type);

            return Packet.Pack(Protocols.GC_ENEMY_DEAD, p);
        }
    }
}