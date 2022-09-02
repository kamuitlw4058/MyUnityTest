using Lockstep.Serialization;

namespace NetMsg.Common {
    [System.Serializable]
    [SelfImplement]
    [Udp]
    public partial class PlayerPing : BaseMsg {
        public byte localId;
        public long sendTimestamp;
        public int Id;
        
        public override void Serialize(Serializer writer){
            writer.Write(localId);
            writer.Write(sendTimestamp);
            writer.Write(Id);
        }

        public override void Deserialize(Deserializer reader){
            localId = reader.ReadByte();
            sendTimestamp = reader.ReadInt64();
            Id = reader.ReadInt32();
        }
    }

    [System.Serializable]
    [SelfImplement]
    [Udp]
    public partial class Msg_C2G_PlayerPing : PlayerPing { }

    [System.Serializable]
    [SelfImplement]
    [Udp]
    public partial class Msg_G2C_PlayerPing : PlayerPing {
        public long timeSinceServerStart;
        public override void Serialize(Serializer writer){
            base.Serialize(writer);
            writer.Write(timeSinceServerStart);
        }

        public override void Deserialize(Deserializer reader){
            base.Deserialize(reader);
            timeSinceServerStart = reader.ReadInt64();
        }
    }

    [System.Serializable]
    [SelfImplement]
    [Udp]
    public partial class ServerFrame : BaseMsg {
        public byte[] inputDatas; //包含玩家的输入& 游戏输入
        public int tick;
        public Msg_PlayerInput[] allMsgPlayerInputs;

        public Msg_PlayerInput[] AllMsgPlayerInputs {
            get { return allMsgPlayerInputs; }
            set {
                allMsgPlayerInputs = value;
                inputDatas = null;
            }
        }

        private byte[] _serverInputs;
        
        public byte[] ServerInputs {//服务器输入 如掉落等
            get { return _serverInputs; }
            set {
                _serverInputs = value;
                inputDatas = null;
            }
        }

        public void BeforeSerialize(){
            if (inputDatas != null) return;
            var writer = new Serializer();
            var inputLen = (byte) (AllMsgPlayerInputs?.Length ?? 0);
            writer.Write(inputLen);
            for (byte i = 0; i < inputLen; i++) {
                var cmd = AllMsgPlayerInputs[i].Command;
                cmd.Serialize(writer);
            }

            writer.WriteBytes_255(_serverInputs);
            inputDatas = writer.CopyData();
        }

        public void AfterDeserialize()
        {
            var reader = new Deserializer(inputDatas);
            var inputLen = reader.ReadByte();
            allMsgPlayerInputs = new Msg_PlayerInput[inputLen];
            for (byte i = 0; i < inputLen; i++)
            {
                var input = new Msg_PlayerInput();
                input.Tick = tick;
                input.ActorId = i;
                allMsgPlayerInputs[i] = input;
                var cmd = new InputCmd();
                cmd.Deserialize(reader);
                input.Command = cmd;
            }

            _serverInputs = reader.ReadBytes_255();
        }

        public override void Serialize(Serializer writer){
            BeforeSerialize();
            writer.Write(tick);
            writer.Write(inputDatas);
        }

        public override void Deserialize(Deserializer reader){
            tick = reader.ReadInt32();
            inputDatas = reader.ReadBytes();
            AfterDeserialize();
        }

        public override string ToString(){
            var count = (inputDatas == null) ? 0 : inputDatas.Length;
            return
                $"t:{tick} " +
                $"inputNum:{count}";
        }

        public override bool Equals(object obj){
            if (obj == null) return false;
            var frame = obj as ServerFrame;
            return Equals(frame);
        }

        public override int GetHashCode(){
            return tick;
        }

        public bool Equals(ServerFrame frame){
            if (frame == null) return false;
            if (tick != frame.tick) return false;
            BeforeSerialize();
            frame.BeforeSerialize();
            return inputDatas.EqualsEx(frame.inputDatas);
        }
    }
}