﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace projet_intégration
{
    class BroadcastClient
    {
        const int udpRangeStart = 15123;
        const int localMaximumPortCount = 16;
        UdpClient udpClient;
        IPEndPoint udpReceiveEndPoint;
        List<IPEndPoint> udpSendEndPoints;
        public int LocalPort;
        public bool IsListening = false;
        public Queue<Message> MessagesReceived = new Queue<Message>();

        /// <summary>
        /// constructeur
        /// </summary>
        public BroadcastClient()
        {
            BeginListening();
            SetupSendPorts();
        }

        private void BeginListening()
        {
            var portTestCount = 0;
            var udpPortFound = false;
            do
            {
                try
                {
                    LocalPort = udpRangeStart + portTestCount;
                    udpReceiveEndPoint = new IPEndPoint(
                    IPAddress.Any, LocalPort);
                    udpClient = new UdpClient(udpReceiveEndPoint);
                    udpPortFound = true;
                }
                catch (SocketException)
                {
                    portTestCount++;
                }
            } while (!udpPortFound && portTestCount < localMaximumPortCount);
            if (udpPortFound)
            {
                udpClient.BeginReceive(UdpMessageReceived, udpClient);
                IsListening = true;
            }
        }

        private void UdpMessageReceived(IAsyncResult asyncResult)
        {
            var receivedBytes = udpClient.EndReceive(
            asyncResult,
            ref udpReceiveEndPoint);
            udpClient.BeginReceive(UdpMessageReceived, udpClient);
            if (udpReceiveEndPoint.Port != LocalPort)
            {
                MessagesReceived.Enqueue(
                new Message()
                {
                    Address = udpReceiveEndPoint.Address,
                    Port = udpReceiveEndPoint.Port,
                    Bytes = receivedBytes
                });
            }
        }

        private void SetupSendPorts()
        {
            udpSendEndPoints = new List<IPEndPoint>();
            for (var sendPortOffset = 0;
            sendPortOffset < localMaximumPortCount;
            sendPortOffset++)
            {
                udpSendEndPoints.Add(
                new IPEndPoint(
                IPAddress.Broadcast,
                udpRangeStart + sendPortOffset));
            }
        }

        public void Send(byte[] data)
        {
            foreach (var endpoint in udpSendEndPoints)
            {
                udpClient.BeginSend(
                data,
                data.Length,
                endpoint,
                UdpMessageSent,
                udpClient);
            }
        }
        private void UdpMessageSent(IAsyncResult asyncResult)
        {
        }
    }
}
