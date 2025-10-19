using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPGM : ServerPacket
    {
        public uint m_iUserId;

        public Vector3 m_Position;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            m_Position.ToWriter(p);

            return Packet.Pack(Protocols.GC_PGM_FIRE, p);
        }
    }
}