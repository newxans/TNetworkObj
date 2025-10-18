using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GTakeMedkit : ServerPacket
    {
        public uint m_iActionRevived;

        public uint m_iUserRevived;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iActionRevived);
            p.wuint(m_iUserRevived);

            return Packet.Pack(Protocols.GC_TAKE_USER_MEDKIT, p);
        }
    }
}