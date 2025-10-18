using NetworkObj.Utils;

namespace NetworkObj.Packets
{
    class GPlayerMove : ServerPacket
    {
        public uint m_iUserId;

        public Vector3 pos;

        public Vector3 rot;

        public Vector3 dir;

        public ulong m_iPingTime;

        public Writer Pack()
        {
            Writer p = new Writer();
            p.wuint(m_iUserId);
            pos.ToWriter(p);
            rot.ToWriter(p);
            dir.ToWriter(p);
            p.wulong(m_iPingTime);

            return Packet.Pack(Protocols.GC_USER_STATUS, p);
        }
    }
}