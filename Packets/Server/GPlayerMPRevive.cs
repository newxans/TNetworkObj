using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPlayerMPRevive : ServerPacket
    {
        public uint m_iResult;

        public uint m_iUserId;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iResult);
            p.wuint(m_iUserId);

            return Packet.Pack(Protocols.GC_USER_REVIVE_MP, p);
        }
    }
}