﻿using Client.Services.Network.Base;
using CrossLibrary.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using static CrossLibrary.Globals;

namespace Client.Services.Network.Udp
{
    //public class UdpService : ProtocolService
    //{
    //    private Socket socket;
    //    private EndPoint remoteEndPoint;

    //    /// <param name="port">Locac port</param>
    //    /// <param name="handle">Delegate to handle incomming message strings</param>
    //    public UdpService(int port, Action<string> handle) : base(port, handle)
    //    {
    //        destination = ServerDestination;
    //    }

    //    public override void Send(byte[] data)
    //    {
    //        IPEndPoint remoteIP = new(IPAddress.Parse(destination.Ip), destination.Port);
    //        socket.SendTo(data, remoteIP);
    //    }

    //    public override void Start()
    //    {
    //        if (!run)
    //        {
    //            run = true;
    //            socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    //            IPEndPoint localEndPoint = new(IPAddress.Parse("127.0.0.1"), port);
    //            socket.Bind(localEndPoint);

    //            listenTask = new(Listen);
    //            listenTask.Start();
    //        }
    //    }

    //    public override void Stop()
    //    {
    //        if (run)
    //        {
    //            run = false;
    //            socket.Shutdown(SocketShutdown.Both);
    //            socket.Close();
    //        }
    //    }

    //    /// <summary>
    //    /// Loop that read incomming responses
    //    /// </summary>
    //    protected override void Listen()
    //    {
    //        try
    //        {
    //            while (run)
    //            {
    //                remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
    //                string message = Receive();
    //                handle(message);
    //            }
    //        }
    //        catch (SocketException socketEx)
    //        {
    //            if (socketEx.ErrorCode != 10004)
    //                MessageBox.Show(socketEx.Message + socketEx.ErrorCode);
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show(ex.Message + ex.GetType().ToString());
    //        }
    //        finally
    //        {
    //            Stop();
    //        }
    //    }

    //    protected string Receive()
    //    {
    //        int bytes;
    //        byte[] buffer = new byte[bufferSize];
    //        StringBuilder builder = new();
    //        do
    //        {
    //            bytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);
    //            builder.Append(CommunicationEncoding.GetString(buffer, 0, bytes));
    //        } while (socket.Available > 0);

    //        return builder.ToString();
    //    }
    //}
}