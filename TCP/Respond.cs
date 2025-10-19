using NetworkObj.Packets;
using NetworkObj.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Reflection.Metadata;

namespace NetworkObj.TCP;

class Responder
{
    private Reader rpacket = new Reader(new byte[0]);
    private TcpClient client = new TcpClient();

    public async Task Respond(TcpClient cli)
    {
        client = cli;
        Clients.AddClient(client);

        NetworkStream stream = client.GetStream();
        byte[] lengthBuffer = new byte[4];

        while (true)
        {
            if (!await Reader.rexact(stream, lengthBuffer, 0, 4)) return;

            Reader length = new Reader(lengthBuffer);
            uint len = length.ruint();

            if (len < 0 || len > 256) return; // i dont believe triniti usually sends packets even over 100 bytes

            byte[] buffer = new byte[len];
            Array.Copy(lengthBuffer, 0, buffer, 0, 4);

            int tr = (int)len - 4;
            if (tr > 0)
            {
                if (!await Reader.rexact(stream, buffer, 4, tr)) return;
            }

            if (!new Reader(buffer).parse(buffer)) Logger.Error("Packet parser failed");

            rpacket = new Reader(buffer);
            uint packetType = rpacket.ptype();

            if (Clients.GetUser(client) == null)
            {
                Logger.Warning($"Breaking {client.Client.RemoteEndPoint}'s connection (No user connected to client)");
                client.Close();
                break;
            }

            switch ((Protocols)packetType)
            {
                case Protocols.CG_HEARTBEAT:
                    await Heartbeat();
                    break;
                case Protocols.CG_CREATE_ROOM:
                    await CreateRoom();
                    break;
                case Protocols.CG_START_GAME:
                    await StartRoom();
                    break;
                case Protocols.CG_DESTROY_ROOM:
                    await DestroyRoom();
                    break;
                case Protocols.CG_LEAVE_ROOM:
                    await LeaveRoom();
                    break;
                case Protocols.CG_ROOM_INFO:
                    await RoomInfo();
                    break;
                case Protocols.CG_JOIN_ROOM:
                    await JoinRoom();
                    break;
                case Protocols.CG_USER_SPAWN:
                    await PlayerSpawn();
                    break;
                case Protocols.CG_USER_ACTION:
                    await PlayerAction();
                    break;
                case Protocols.CG_USER_BONUS_ACTION:
                    await PlayerBonusAction();
                    break;
                case Protocols.CG_USER_STATUS:
                    await PlayerMove();
                    break;
                case Protocols.CG_USER_INJURED:
                    await PlayerInjury();
                    break;
                case Protocols.CG_USER_REVIVE:
                    await PlayerRevive();
                    break;
                case Protocols.CG_USER_REVIVE_MP:
                    await PlayerMPRevive();
                    break;
                case Protocols.CG_USER_CHANGE_WEAPON:
                    await PlayerChangeWeapon();
                    break;
                case Protocols.CG_PGM_FIRE:
                    await PGM();
                    break;
                case Protocols.CG_ENEMY_SPAWN:
                    await EnemySpawn();
                    break;
                case Protocols.CG_ENEMY_CHANGE_TARGET:
                    await EnemyTarget();
                    break;
                case Protocols.CG_ENEMY_DEAD:
                    await EnemyDead();
                    break;
                case Protocols.CG_ENEMY_INJURED:
                    await EnemyInjured();
                    break;
                case Protocols.CG_ENEMY_LOOT_NEW:
                    await EnemyLootNew();
                    break;
                case Protocols.CG_ENEMY_LOOT:
                    await EnemyLoot();
                    break;
                case Protocols.CG_ENEMY_REMOVE:
                    await EnemyRemove();
                    break;
                case Protocols.CG_ENEMY_STATUS:
                    await EnemyStatus();
                    break;
                case Protocols.CG_USER_DEAD:
                    await Dead();
                    break;
                case Protocols.CG_GAME_OVER:
                    await GameOver();
                    break;
                case Protocols.CG_COOP_WINNER:
                    await Win();
                    break;
                case Protocols.CG_KICK_USER:
                    await Kick();
                    break;
                default:
                    Logger.Error($"{Enum.GetName(typeof(Protocols), (Protocols)packetType)} unimplemented");
                    break;
            }
        }
    }

    async Task Heartbeat()
    {
        GHeartbeat p = new GHeartbeat();
        p.m_lLocalTime = rpacket.rulong();

        Writer pack = p.Pack();

        await Clients.SendToClient(client, pack);
    }

    async Task CreateRoom()
    {
        GCreateRoom p = new GCreateRoom();
        uint mapId = rpacket.ruint();
        ulong localtime = rpacket.rulong();
        string nickname = rpacket.rstring();
        uint avatar = rpacket.ruint();
        uint days = rpacket.ruint();
        string password = rpacket.rstring();

        User host = Clients.GetUser(client);
        host.RoomMaster = true;
        host.Name = nickname;
        host.Avatar = (int)avatar;
        host.Level = (int)days;

        int RoomId = Rooms.CreateRoom(client, password);
        Rooms.GetRoom(RoomId).MapId = (int)mapId;

        host.RoomId = RoomId;
        p.m_iResult = 0u;
        p.m_iUserId = (uint)host.UserId;
        p.m_iRoomId = (uint)RoomId;
        p.m_lLocalTime = (long)localtime;
        p.m_lServerTime = (long)localtime;

        if (RoomId == -1 || RoomId == -2)
        {
            p.m_iResult = 1u;
            host.RoomMaster = false;
            host.RoomId = -1;
        }

        await Clients.SendToClient(client, p.Pack());
    }

    async Task StartRoom()
    {
        User host = Clients.GetUser(client);
        if (host.RoomId == -1 || !host.RoomMaster) return;

        Logger.Log($"Room {host.RoomId} started");

        await Rooms.SendToRoom(host.RoomId, DefaultPacket(Protocols.GC_START_GAME_NOTIFY));
        await Rooms.SendToRoom(host.RoomId, DefaultPacket(Protocols.GC_START_GAME));
    }

    async Task DestroyRoom()
    {
        User host = Clients.GetUser(client);

        if (host.RoomId == -1 || !host.RoomMaster) return;

        Logger.Log($"Room {host.RoomId} destroyed");

        await Rooms.SendToRoom(host.RoomId, DefaultPacket(Protocols.GC_DESTROY_ROOM, true));
        await Rooms.DeleteRoom(host.RoomId);
    }

    async Task LeaveRoom()
    {
        User user = Clients.GetUser(client);
        if (user.RoomId == -1) return;

        GLeaveRoom notify = new GLeaveRoom();
        notify.m_iUserId = (uint)user.UserId;

        Room? r = Rooms.GetRoom(user.RoomId);

        r.Online = r.Online - 1;

        if (Rooms.GetRoom(user.RoomId)?.Online == 0)
        {
            await Rooms.DeleteRoom(user.RoomId);
        }

        await Rooms.SendToRoom(user.RoomId, notify.Pack());
        await Rooms.LeaveRoom(user.RoomId, client);
        await Clients.SendToClient(client, DefaultPacket(Protocols.GC_LEAVE_ROOM, true));
    }

    async Task RoomInfo()
    {
        uint roomId = rpacket.ruint();

        Room? room = Rooms.GetRoom((int)roomId);

        if (room == null) return;

        User creator = Clients.GetUser(room.Players[0]);

        GRoomInfo p = new GRoomInfo();

        p.m_iResult = 0u;
        p.m_iMapId = (uint)room.MapId;
        p.m_room_status = 0u;
        p.m_password = room.Password;
        p.m_strCreaterNickname = creator.Name;
        p.m_Creater_level = (uint)creator.Level;
        p.m_iOnlineNum = (uint)room.Online;
        p.m_iMaxUserNum = (uint)room.Max;
        p.m_iRoomId = roomId;

        await Clients.SendToClient(client, p.Pack());
    }

    async Task JoinRoom()
    {
        uint roomId = rpacket.ruint();
        ulong localTime = rpacket.rulong();
        string name = rpacket.rstring();
        uint avt = rpacket.ruint();
        uint days = rpacket.ruint();

        User user = Clients.GetUser(client);
        Room? room = Rooms.GetRoom((int)roomId);

        GJoinRoom p = new GJoinRoom();
        if (room == null)
        {
            p.m_iResult = 2u;
            p.m_iRoomId = 0u;
        }
        else if (room.Online == 4)
        {
            p.m_iResult = 1u;
            p.m_iRoomId = 0u;
        }
        else
        {
            p.m_iResult = 0u;
            p.m_iRoomId = roomId;
        }

        p.m_map_id = (uint)room.MapId;
        p.m_lLocalTime = (long)localTime;
        p.m_lServerTime = (long)localTime;
        p.m_room_index = (uint)room.Online;
        p.m_iUserId = (uint)user.UserId;

        user.Index = room.Online;
        user.Name = name;
        user.Avatar = (int)avt;
        user.Level = (int)days;
        user.RoomMaster = false;
        user.RoomId = (int)roomId;

        room.Online = room.Online + 1;
        room.Players.Add(client);

        room.Players.ForEach(async (TcpClient rando) => {
            if (!(client == rando))
            {
                User ruser = Clients.GetUser(rando);
                GJoinRoomNotify notify2 = new GJoinRoomNotify();
                notify2.m_room_index = (uint)ruser.Index;
                notify2.m_strNickname = ruser.Name;
                notify2.m_iLevel = (uint)ruser.Level;
                notify2.m_iAvatarType = (uint)ruser.Avatar;
                notify2.m_iUserId = (uint)ruser.UserId;
                await Clients.SendToClient(client, notify2.Pack());
            }
        });

        GJoinRoomNotify notify = new GJoinRoomNotify();

        notify.m_room_index = (uint)user.Index;
        notify.m_strNickname = user.Name;
        notify.m_iLevel = (uint)user.Level;
        notify.m_iAvatarType = (uint)user.Avatar;
        notify.m_iUserId = (uint)user.UserId;

        await Clients.SendToClient(client, p.Pack());
        await Rooms.SendToRoom((int)roomId, notify.Pack(), client);

        Logger.Log($"User {user.UserId} join Room {roomId}");
    }

    async Task PlayerSpawn()
    {
        ulong localTime = rpacket.rulong();
        uint bpIndex = rpacket.ruint();
        uint wp1 = rpacket.ruint();
        uint wp2 = rpacket.ruint();
        uint wp3 = rpacket.ruint();

        User user = Clients.GetUser(client);

        GPlayerSpawn p = new GPlayerSpawn();

        p.m_lLocalTime = (long)localTime;
        p.m_lServerTime = (long)localTime;
        p.m_iUserId = (uint)user.UserId;
        p.m_iBirthPointIndex = (uint)user.Index;
        p.m_iWeaponIndex1 = wp1;
        p.m_iWeaponIndex2 = wp2;
        p.m_iWeaponIndex3 = wp3;

        await Rooms.SendToRoom(user.RoomId, p.Pack(), client);
    }

    async Task PlayerAction()
    {
        uint userId = rpacket.ruint();
        uint actionId = rpacket.ruint();

        GPlayerAction p = new GPlayerAction();

        p.m_iUserId = userId;
        p.m_iAction = actionId;

        await Rooms.SendToRoom(Clients.GetUser(client).RoomId, p.Pack(), client);
    }

    async Task PlayerBonusAction()
    {
        uint userId = rpacket.ruint();
        uint auxActionId = rpacket.ruint();

        GPlayerBonusAction p = new GPlayerBonusAction();

        p.m_iUserId = userId;
        p.m_iBonusAction = auxActionId;

        await Rooms.SendToRoom(Clients.GetUser(client).RoomId, p.Pack(), client);
    }

    async Task PlayerMove() // credit: overmet15 for vector struct otherwise sync would be TERRIBLE
    {
        uint userId = rpacket.ruint();

        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        Vector3 rot = new Vector3();
        rot.FromReader(rpacket);

        Vector3 dir = new Vector3();
        dir.FromReader(rpacket);

        ulong local = rpacket.rulong();

        GPlayerMove p = new GPlayerMove();

        p.m_iUserId = userId;
        p.pos = pos;
        p.rot = rot;
        p.dir = dir;
        p.m_iPingTime = local;

        await Rooms.SendToRoom(Clients.GetUser(client).RoomId, p.Pack(), client);
    }

    async Task PlayerInjury()
    {
        uint userId = rpacket.ruint();
        ulong hit1 = rpacket.rulong();
        ulong hit2 = rpacket.rulong();
        ulong hit3 = rpacket.rulong();

        GPlayerInjury p = new GPlayerInjury();

        p.m_iUserId = userId;
        p.m_iInjury_val = (long)hit1;
        p.m_total_hp_val = (long)hit2;
        p.m_cur_hp_val = (long)hit3;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task PlayerRevive()
    {
        uint userId = rpacket.ruint();

        GStandard p = new GStandard();
        p.m_iUserId = userId;
        p.protocol = Protocols.GC_USER_REVIVE;

        Room y = Rooms.GetRoom(GetRoomId(client));
        y.Dead = y.Dead - 1;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task PlayerMPRevive()
    {
        uint action = rpacket.ruint();
        uint userId = rpacket.ruint();

        GPlayerMPRevive p = new GPlayerMPRevive();
        p.m_iResult = 0u;
        p.m_iUserId = userId;

        GTakeMedkit take = new GTakeMedkit();
        take.m_iActionRevived = action;
        take.m_iUserRevived = userId;

        Room y = Rooms.GetRoom(GetRoomId(client));
        y.Dead = y.Dead - 1;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack());
        await Rooms.SendToRoom(GetRoomId(client), take.Pack());
    }

    async Task PlayerChangeWeapon()
    {
        uint userId = rpacket.ruint();
        uint weaponIndex = rpacket.ruint();

        GChangeWeapon p = new GChangeWeapon();
        p.m_iUserId = userId;
        p.m_iWeaponIndex = weaponIndex;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task PGM()
    {
        uint userId = rpacket.ruint();
        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        GPGM p = new GPGM();
        p.m_iUserId = userId;
        p.m_Position = pos;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemySpawn()
    {
        uint day = rpacket.ruint();
        uint enemy = rpacket.ruint();
        uint type = rpacket.ruint();
        uint elite = rpacket.ruint();
        uint grave = rpacket.ruint();
        uint boss = rpacket.ruint();

        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        uint target = rpacket.ruint();

        GEnemySpawn p = new GEnemySpawn();

        p.m_enemy_wave = day;
        p.m_enemy_Id = enemy;
        p.m_enemy_type = type;
        p.m_isElite = elite;
        p.m_isGrave = grave;
        p.m_isSuperBoss = boss;
        p.m_Position = pos;
        p.m_target_id = target;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyTarget()
    {
        string enemyId = rpacket.rstring();
        uint target = rpacket.ruint();

        GEnemyTarget p = new GEnemyTarget();
        p.m_enemyID = enemyId;
        p.target_id = target;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyDead()
    {
        uint playerId = rpacket.ruint();
        string enemyId = rpacket.rstring();
        uint type = rpacket.ruint();
        uint elite = rpacket.ruint();
        uint weapon = rpacket.ruint();

        GEnemyDead p = new GEnemyDead();

        p.m_iResult = 0u;
        p.m_enemy_type = type;
        p.bElite = elite;
        p.weapon_type = weapon;

        GEnemyDeadNotify not = new GEnemyDeadNotify();

        not.enemy_id = enemyId;
        not.iPlayerId = playerId;
        not.m_enemy_type = type;
        not.weapon_type = weapon;
        not.bElite = elite;

        await Clients.SendToClient(client, p.Pack());
        await Rooms.SendToRoom(GetRoomId(client), not.Pack(), client);
    }

    async Task EnemyInjured()
    {
        string enemyId = rpacket.rstring();
        ulong damage = rpacket.rulong();
        uint wpndmg = rpacket.ruint();
        uint crit = rpacket.ruint();

        GEnemyInjured p = new GEnemyInjured();

        p.m_enemyID = enemyId;
        p.m_iDamage = (long)damage;
        p.m_weapon_type = wpndmg;
        p.m_critical_attack = crit;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyLootNew()
    {
        uint type = rpacket.ruint();
        uint id = rpacket.ruint();

        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        GEnemyLootNew p = new GEnemyLootNew();

        p.item_type = type;
        p.id = id;
        p.m_Position = pos;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyLoot()
    {
        uint type = rpacket.ruint();

        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        GEnemyLoot p = new GEnemyLoot();

        p.item_type = type;
        p.m_Position = pos;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyRemove()
    {
        string enemyId = rpacket.rstring();

        GEnemyRemove p = new GEnemyRemove();
        p.m_enemyID = enemyId;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task EnemyStatus()
    {
        string enemyId = rpacket.rstring();

        Vector3 pos = new Vector3();
        pos.FromReader(rpacket);

        Vector3 rot = new Vector3();
        rot.FromReader(rpacket);

        Vector3 dir = new Vector3();
        dir.FromReader(rpacket);

        GEnemyStatus p = new GEnemyStatus();

        p.m_enemyID = enemyId;
        p.m_Position = pos;
        p.m_Rotation = rot;
        p.m_Direction = dir;

        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task Dead()
    {
        Room y = Rooms.GetRoom(GetRoomId(client));
        y.Dead = y.Dead + 1;

        if (y.Dead == y.Online)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(10000);
                if (y.Dead == y.Online)
                {
                    await Rooms.DeleteRoom(GetRoomId(client));
                }
            });
        }
    }

    async Task GameOver()
    {
        uint userId = rpacket.ruint(); // TODO: Anti-Cheat that disconnects player that sends any movement whilst dead.
        GStandard p = new GStandard();
        p.m_iUserId = userId;
        p.protocol = Protocols.GC_GAME_OVER;
        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task Win()
    {
        uint winnerId = rpacket.ruint();
        GStandard p = new GStandard();
        p.m_iUserId = winnerId;
        p.protocol = Protocols.GC_COOP_WINNER;
        await Rooms.SendToRoom(GetRoomId(client), p.Pack(), client);
    }

    async Task Kick()
    {
        uint userId = rpacket.ruint();
        Room room = Rooms.GetRoom(GetRoomId(client));

        if (room == null || !Clients.GetUser(client).RoomMaster) return;

        GStandard notify = new GStandard();

        notify.m_iUserId = userId;
        notify.protocol = Protocols.GC_KICK_USER_NOTIFY;

        GPlayerKick p = new GPlayerKick();

        p.m_iResult = 0u;
        p.m_iUserId = userId;

        TcpClient random = new TcpClient();

        room.Players.ForEach(async (TcpClient rando) =>
        {
            User user = Clients.GetUser(rando);

            if (user.UserId == userId)
            {
                random = rando;
            }
        });

        await Rooms.SendToRoom(GetRoomId(client), notify.Pack());
        await Rooms.SendToRoom(GetRoomId(client), p.Pack());
        await Clients.SendToClient(random, DefaultPacket(Protocols.GC_KICKED_BY));
        await Rooms.LeaveRoom(GetRoomId(client), random);
    }

    int GetRoomId(TcpClient c)
    {
        return Clients.GetUser(c).RoomId;
    }

    Writer DefaultPacket(Protocols packetType, bool result = false)
    {
        Writer packet = new Writer();

        if (result) packet.wuint(16u);
        if (!result) packet.wuint(12u);

        packet.wuint((uint)packetType);
        packet.wuint(1u);

        if (result) packet.wuint(0u);

        return packet;
    }
}