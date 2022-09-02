using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lockstep.Util;

namespace Lockstep.Network
{
    public struct WaitSendBuffer
    {
        public byte[] Bytes;
        public int Index;
        public int Length;

        public WaitSendBuffer(byte[] bytes, int index, int length)
        {
            this.Bytes = bytes;
            this.Index = index;
            this.Length = length;
        }
    }

    public class KChannel : AChannel
    {
        private UdpClient socket;

        private Kcp kcp;

        private readonly CircularBuffer recvBuffer = new CircularBuffer(8192);
        private readonly Queue<WaitSendBuffer> sendBuffer = new Queue<WaitSendBuffer>();

        private readonly PacketParser parser;
        private bool isConnected;
        private readonly IPEndPoint remoteEndPoint;

        private TaskCompletionSource<Packet> recvTcs;

        private uint lastRecvTime;

        private readonly byte[] cacheBytes = new byte[ushort.MaxValue];

        public uint Conn;

        public uint RemoteConn;
        
        private int m_SendFecId = 0;
        private byte[] m_SendFecMinus0 = new byte[0];
        private byte[] m_SendFecMinus1 = new byte[0];
        private byte[] m_SendFecMinus2 = new byte[0];
        private SortedSet<int> m_RecvFecPackId = new SortedSet<int>();

        // accept
        public KChannel(uint conn, uint remoteConn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService) :
            base(kService, ChannelType.Accept)
        {
            this.Id = conn;
            this.Conn = conn;
            this.RemoteConn = remoteConn;
            this.remoteEndPoint = remoteEndPoint;
            this.socket = socket;
            this.parser = new PacketParser(this.recvBuffer);
            kcp = new Kcp(this.RemoteConn, this.Output);
            kcp.SetMtu(512);
            kcp.NoDelay(1, 10, 2, 1); //fast
            kcp.WndSize(512, 512);
            this.isConnected = true;
            this.lastRecvTime = kService.TimeNow;
        }

        // connect
        public KChannel(uint conn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService) : base(kService,
            ChannelType.Connect)
        {
            this.Id = conn;
            this.Conn = conn;
            this.socket = socket;
            this.parser = new PacketParser(this.recvBuffer);

            this.remoteEndPoint = remoteEndPoint;
            this.lastRecvTime = kService.TimeNow;
            //this.Connect(kService.TimeNow);

            kcp = new Kcp(this.Conn, this.Output);
            kcp.SetMtu(512);
            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(512, 512);
            this.isConnected = true;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            // for (int i = 0; i < 4; i++)
            // {
            //     this.DisConnect();
            // }

            this.socket = null;
        }

        private KService GetService()
        {
            return (KService) this.service;
        }

        public void HandleConnnect(uint responseConn)
        {
            if (this.isConnected)
            {
                return;
            }

            this.isConnected = true;

            this.RemoteConn = responseConn;
            this.kcp = new Kcp(responseConn, this.Output);
            kcp.SetMtu(512);
            kcp.NoDelay(1, 10, 2, 1); //fast

            HandleSend();
        }

        public void HandleAccept(uint requestConn)
        {
            cacheBytes.WriteTo(0, KcpProtocalType.ACK);
            cacheBytes.WriteTo(4, requestConn);
            cacheBytes.WriteTo(8, this.Conn);
            this.socket.Send(cacheBytes, 12, remoteEndPoint);
        }

        /// <summary>
        /// 发送请求连接消息
        /// </summary>
        private void Connect(uint timeNow)
        {
            cacheBytes.WriteTo(0, KcpProtocalType.SYN);
            cacheBytes.WriteTo(4, this.Conn);
            //Log.Debug($"client connect: {this.Conn}");
            this.socket.Send(cacheBytes, 8, remoteEndPoint);

            // 200毫秒后再次update发送connect请求
            this.GetService().AddToNextTimeUpdate(timeNow + 200, this.Id);
        }

        private void DisConnect()
        {
            cacheBytes.WriteTo(0, KcpProtocalType.FIN);
            cacheBytes.WriteTo(4, this.Conn);
            cacheBytes.WriteTo(8, this.RemoteConn);
            //Log.Debug($"client disconnect: {this.Conn}");
            this.socket.Send(cacheBytes, 12, remoteEndPoint);
        }

        public void Update(uint timeNow)
        {
            // 如果还没连接上，发送连接请求
            if (!this.isConnected)
            {
                Connect(timeNow);
                return;
            }

            // 超时断开连接
            // if (timeNow - this.lastRecvTime > 20 * 1000)
            // {
            //     this.OnError(SocketError.Disconnecting);
            //     return;
            // }

            this.kcp.Update(timeNow);
            // uint nextUpdateTime = this.kcp.Check(timeNow);
            uint nextUpdateTime = timeNow;
            this.GetService().AddToNextTimeUpdate(nextUpdateTime, this.Id);
        }

        private void HandleSend()
        {
            while (true)
            {
                if (this.sendBuffer.Count <= 0)
                {
                    break;
                }

                WaitSendBuffer buffer = this.sendBuffer.Dequeue();
                this.KcpSend(buffer.Bytes, buffer.Index, buffer.Length);
            }
        }

        public void HandleRecv(byte[] date, uint timeNow)
        {
            int index = 0;
            int maxPackId = BitConverter.ToInt32(date, index);
            index += 4;
            for (var i = 0; i < 3; i++)
            {
                int packSize = BitConverter.ToInt32(date, index);
                index += 4;
                
                var fecPacket = new byte[packSize];
                Array.Copy(date, index, fecPacket, 0, packSize);
                index += packSize;
                
                int packId = maxPackId - i;

                if (m_RecvFecPackId.Count == 0 || (!m_RecvFecPackId.Contains(packId) && packId > m_RecvFecPackId.Min()))
                {
                    m_RecvFecPackId.Add(packId);
                    this.kcp.Input(fecPacket);
                }
            }


            // 加入update队列
            this.GetService().AddToUpdate(this.Id);

            while (true)
            {
                int n = kcp.PeekSize();
                if (n == 0)
                {
                    this.OnError(SocketError.NetworkReset);
                    return;
                }

                int count = this.kcp.Recv(this.cacheBytes);
                if (count <= 0)
                {
                    return;
                }

                lastRecvTime = timeNow;

                // 收到的数据放入缓冲区
                byte[] sizeBuffer = BitConverter.GetBytes((ushort) (count - 2));
                //this.recvBuffer.Write(sizeBuffer, 0, sizeBuffer.Length);
                this.recvBuffer.Write(cacheBytes, 0, count);

                if (this.recvTcs != null)
                {
                    bool isOK = this.parser.Parse();
                    if (isOK)
                    {
                        Packet pkt = this.parser.GetPacket();
                        var tcs = this.recvTcs;
                        this.recvTcs = null;
                        tcs.SetResult(pkt);
                    }
                }
            }
        }

        public void Output(byte[] bytes, int count)
        {
            m_SendFecMinus2 = m_SendFecMinus1;
            m_SendFecMinus1 = m_SendFecMinus0;
            m_SendFecMinus0 = new byte[count];
            Array.Copy(bytes, 0, m_SendFecMinus0, 0, count);

            var size = m_SendFecMinus0.Length + m_SendFecMinus1.Length + m_SendFecMinus2.Length + 16;
            byte[] fecBytes = new byte[size];
            int index = 0;
            
            byte[] maxPackIdBuffer = BytesHelper.GetBytes(m_SendFecId);
            Array.Copy(maxPackIdBuffer, 0, fecBytes, index, maxPackIdBuffer.Length);
            index += 4;

            byte[] pack1SizeBuffer = BytesHelper.GetBytes(m_SendFecMinus0.Length);
            Array.Copy(pack1SizeBuffer, 0, fecBytes, index, pack1SizeBuffer.Length);
            index += 4;
            Array.Copy(m_SendFecMinus0, 0, fecBytes, index, m_SendFecMinus0.Length);
            index += m_SendFecMinus0.Length;

            byte[] pack2SizeBuffer = BytesHelper.GetBytes(m_SendFecMinus1.Length);
            Array.Copy(pack2SizeBuffer, 0, fecBytes, index, pack2SizeBuffer.Length);
            index += 4;
            Array.Copy(m_SendFecMinus1, 0, fecBytes, index, m_SendFecMinus1.Length);
            index += m_SendFecMinus1.Length;
            
            byte[] pack3SizeBuffer = BytesHelper.GetBytes(m_SendFecMinus2.Length);
            Array.Copy(pack3SizeBuffer, 0, fecBytes, index, pack3SizeBuffer.Length);
            index += 4;
            Array.Copy(m_SendFecMinus2, 0, fecBytes, index, m_SendFecMinus2.Length);

            m_SendFecId++;
            
            this.socket.Send(fecBytes, size, this.remoteEndPoint);
        }

        private void KcpSend(byte[] buffers, int index, int length)
        {
            this.kcp.Send(buffers, index, length);
            this.GetService().AddToUpdate(this.Id);
        }

        public override void Send(byte[] buffer, int index, int length)
        {
            if (isConnected)
            {
                this.KcpSend(buffer, index, length);
                return;
            }

            this.sendBuffer.Enqueue(new WaitSendBuffer(buffer, index, length));
        }

        public override void Send(List<byte[]> buffers)
        {
            ushort size = (ushort) buffers.Select(b => b.Length).Sum();
            byte[] sizeBuffer = BytesHelper.GetBytes(size);
            byte[] bytes;
            if (!this.isConnected)
            {
                bytes = this.cacheBytes;
            }
            else
            {
                bytes = new byte[size + 2];
            }

            int index = 0;
            Array.Copy(sizeBuffer, 0, bytes, index, sizeBuffer.Length);
            index += 2;
            foreach (byte[] buffer in buffers)
            {
                Array.Copy(buffer, 0, bytes, index, buffer.Length);
                index += buffer.Length;
            }

            Send(bytes, 0, size + 2);
        }

        public override Task<Packet> Recv()
        {
            if (this.IsDisposed)
            {
                throw new Exception("KChannel已经被Dispose, 不能接收消息");
            }

            bool isOK = this.parser.Parse();
            if (isOK)
            {
                Packet packet = this.parser.GetPacket();
                return Task.FromResult(packet);
            }

            recvTcs = new TaskCompletionSource<Packet>();
            return recvTcs.Task;
        }
    }
}