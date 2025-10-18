using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPlayerInjury : ServerPacket
    {
        public uint m_iUserId;

        public long m_iInjury_val;

        public long m_total_hp_val;

        public long m_cur_hp_val;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            p.wulong((ulong)m_iInjury_val);
            p.wulong((ulong)m_total_hp_val);
            p.wulong((ulong)m_cur_hp_val);

            return Packet.Pack(Protocols.GC_USER_INJURED, p);
        }
    }
}