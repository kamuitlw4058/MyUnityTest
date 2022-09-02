using System;
using Lockstep.Serialization;

namespace NetMsg.Common {

    public interface IBaseMsg { }
    [System.Serializable]
    [SelfImplement]
    public partial class BaseMsg : BaseFormater ,IBaseMsg{}
    
    #region UDP

    [Serializable]
    [SelfImplement]
    [Udp]
    public partial class Msg_ServerFrames : MutilFrames { }

    [Udp]
    public partial class Msg_HashCode : BaseMsg {
        public int StartTick;
        public int[] HashCodes;
    }

    #endregion

    #region TCP

    public partial class IPEndInfo : BaseMsg {
        public string Ip;
        public ushort Port;
    }

    public partial class Msg_G2C_Hello : BaseMsg {
        public byte LocalId;
        public byte UserCount;
        public int MapId;
        public int GameId;
        public int Seed;
        public IPEndInfo UdpEnd;
    }

    public partial class Msg_G2C_GameStartInfo : BaseMsg {
        public byte LocalId;
        public byte UserCount;
        public int MapId;
        public int RoomId;
        public int Seed;
        public GameData[] UserInfos;
        public IPEndInfo UdpEnd;
        public IPEndInfo TcpEnd;
        public int SimulationSpeed;
    }
    
    public partial class Msg_C2L_JoinRoom : BaseMsg {
        public int RoomId;
    }

    public partial class Msg_C2G_LoadingProgress : BaseMsg {
        /// [进度百分比 1表示1% 100表示已经加载完成]
        public byte Progress;
    }

    public partial class Msg_G2C_LoadingProgress : BaseMsg {
        /// [进度百分比 1表示1% 100表示已经加载完成]
        public byte[] Progress;
    }

    public partial class Msg_G2C_AllFinishedLoaded : BaseMsg {
        public short Level;
    }

    #endregion
}