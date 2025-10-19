using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyTarget : ServerPacket
    {
        public string? m_enemyID;

        public uint target_id;

        public Writer Pack()
        {
            Writer p = new Writer();
            if (m_enemyID == null) return Packet.Pack(Protocols.CG_KICK_USER, p);
            byte[] enemyId = Encoding.ASCII.GetBytes(m_enemyID);
            p.wuint((uint)enemyId.Length);
            p.wbytes(enemyId);
            p.wuint(target_id);

            return Packet.Pack(Protocols.GC_ENEMY_CHANGE_TARGET, p);
        }
    }
}