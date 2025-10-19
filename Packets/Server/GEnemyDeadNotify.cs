using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyDeadNotify : ServerPacket
    {
        public uint iPlayerId;

        public string? enemy_id;

        public uint m_enemy_type;

        public uint bElite;

        public uint weapon_type;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(iPlayerId);
            if (enemy_id == null) return Packet.Pack(Protocols.CG_KICK_USER, p);
            byte[] enemyId = Encoding.ASCII.GetBytes(enemy_id);
            p.wuint((uint)enemyId.Length);
            p.wbytes(enemyId);
            p.wuint(m_enemy_type);
            p.wuint(bElite);
            p.wuint(weapon_type);

            return Packet.Pack(Protocols.GC_ENEMY_DEAD_NOTIFY, p);
        }
    }
}