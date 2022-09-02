using System.Collections.Generic;
using Lockstep.Serialization;

namespace NetMsg.Common {
    [System.Serializable]
    [SelfImplement]
    [Udp]
    public partial class Msg_PlayerInput : BaseMsg {
        public bool IsMiss;
        public byte ActorId;
        public int Tick;
        public InputCmd Command;
        public float TimeSinceStartUp;


        public Msg_PlayerInput(int tick, byte actorId, InputCmd input)
        {
            Tick = tick;
            ActorId = actorId;
            Command = input;
        }

        public Msg_PlayerInput()
        {
            
        }

        public override void Serialize(Serializer writer){
            writer.Write(TimeSinceStartUp);
            writer.Write(IsMiss);
            writer.Write(ActorId);
            writer.Write(Tick);
            Command.Serialize(writer);
        }

        public override void Deserialize(Deserializer reader){
            TimeSinceStartUp = reader.ReadSingle();
            IsMiss = reader.ReadBoolean();
            ActorId = reader.ReadByte();
            Tick = reader.ReadInt32();
            Command = reader.Parse<InputCmd>();
        }
    }
}