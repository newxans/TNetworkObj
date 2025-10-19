using NetworkObj.Utils;
using System.Buffers;
using System.Net.Sockets;

namespace NetworkObj
{
    public class User
    {
        public int UserId = -1;
        public int RoomId = -1;
        public bool RoomMaster = false;
        public int Avatar = 1;
        public int Level = 1;
        public string Name = "Player";
        public int Index = 0;
        // private int Kills; (survival)
    }

    public class Room
    {
        public int MapId = -1;
        public int Online = 1;
        public string Password = string.Empty;
        public int Max = 4; // TODO: change to 3 if client is in survival (detect using 1u packet)
        public List<TcpClient> Players = new List<TcpClient>(4);
        public int Dead = 0;
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public bool FromReader(Reader reader)
        {
            uint xRaw = reader.ruint();
            uint yRaw = reader.ruint();
            uint zRaw = reader.ruint();

            x = (float)(int)xRaw / 100f;
            y = (float)(int)yRaw / 100f;
            z = (float)(int)zRaw / 100f;

            return true;
        }

        public void ToWriter(Writer writer)
        {
            writer.wuint(unchecked((uint)(int)(x * 100)));
            writer.wuint(unchecked((uint)(int)(y * 100)));
            writer.wuint(unchecked((uint)(int)(z * 100)));
        }

        public override string ToString()
        {
            return $"{{{x}, {y}, {z}}}";
        }
    }
}