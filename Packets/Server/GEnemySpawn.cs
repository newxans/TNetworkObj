using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GEnemySpawn : ServerPacket
    {
        public uint m_enemy_wave;

        public uint m_enemy_Id;

        public uint m_enemy_type;

        public uint m_isElite;

        public uint m_isGrave;

        public uint m_isSuperBoss;

        public Vector3 m_Position;

        public uint m_target_id;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_enemy_wave);
            p.wuint(m_enemy_Id);
            p.wuint(m_enemy_type);
            p.wuint(m_isElite);
            p.wuint(m_isGrave);
            p.wuint(m_isSuperBoss);
            m_Position.ToWriter(p);
            p.wuint(m_target_id);

            return Packet.Pack(Protocols.GC_ENEMY_SPAWN, p);
        }
    }
}