using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPlayerBonusAction : ServerPacket
    {
        public uint m_iUserId;

        public uint m_iBonusAction;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            p.wuint(m_iBonusAction);

            return Packet.Pack(Protocols.GC_USER_BONUS_ACTION, p);
        }
    }
}