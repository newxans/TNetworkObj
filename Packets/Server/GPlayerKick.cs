using NetworkObj.Utils;
using System.Text;

namespace NetworkObj.Packets
{
    class GPlayerKick : ServerPacket
    {
        public uint m_iResult;

        public uint m_iUserId;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iResult);
            p.wuint(m_iUserId);

            return Packet.Pack(Protocols.GC_KICK_USER, p);
        }
    }
}