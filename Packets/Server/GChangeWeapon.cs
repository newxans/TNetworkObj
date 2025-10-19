using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GChangeWeapon : ServerPacket
    {
        public uint m_iUserId;

        public uint m_iWeaponIndex;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            p.wuint(m_iWeaponIndex);

            return Packet.Pack(Protocols.GC_USER_CHANGE_WEAPON, p);
        }
    }
}