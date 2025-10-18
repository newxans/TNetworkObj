using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPlayerAction : ServerPacket
    {
        public uint m_iUserId;

        public uint m_iAction;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            p.wuint(m_iAction);

            return Packet.Pack(Protocols.GC_USER_ACTION, p);
        }
    }
}