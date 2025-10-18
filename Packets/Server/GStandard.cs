using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GStandard : ServerPacket
    {
        public uint m_iUserId;

        public Protocols protocol;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);

            return Packet.Pack(protocol, p);
        }
    }
}