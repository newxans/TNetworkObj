using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GEnemyStatus : ServerPacket
    {
        public string? m_enemyID;

        public Vector3 m_Position;

        public Vector3 m_Rotation;

        public Vector3 m_Direction;

        public Writer Pack()
        {
            Writer p = new Writer();
            if (m_enemyID == null) return Packet.Pack(Protocols.CG_KICK_USER, p);
            byte[] enemyId = Encoding.ASCII.GetBytes(m_enemyID);
            p.wuint((uint)enemyId.Length);
            p.wbytes(enemyId);
            m_Position.ToWriter(p);
            m_Rotation.ToWriter(p);
            m_Direction.ToWriter(p);

            return Packet.Pack(Protocols.GC_ENEMY_STATUS, p);
        }
    }
}