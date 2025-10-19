using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyInjured : ServerPacket
    {
        public string? m_enemyID;

        public long m_iDamage;

        public uint m_weapon_type;

        public uint m_critical_attack;

        public Writer Pack()
        {
            Writer p = new Writer();
            if (m_enemyID == null) return Packet.Pack(Protocols.CG_KICK_USER, p);
            byte[] enemyId = Encoding.ASCII.GetBytes(m_enemyID);
            p.wuint((uint)enemyId.Length);
            p.wbytes(enemyId);
            p.wulong((ulong)m_iDamage);
            p.wuint(m_weapon_type);
            p.wuint(m_critical_attack);

            return Packet.Pack(Protocols.GC_ENEMY_INJURED, p);
        }
    }
}