using System;
using System.Threading;
using Bully.Core.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Bully.Core
{
    public class MessageListener
    {
        private readonly ILogger logger;
        public event EventHandler<int> LeaderChanged; 

        public MessageListener(ILogger logger)
        {
            this.logger = logger;
        }

        public void Listen(CancellationToken cancellationToken)
        {
            using (var electionServer = new ResponseSocket())
            using (var leaderServer = new ResponseSocket())
            using (var pingServer = new ResponseSocket ())
            using (var poller = new NetMQPoller { electionServer , leaderServer, pingServer })
            {
                electionServer.Bind(Settings.ElectionListenerEndpoint);
                leaderServer.Bind(Settings.LeaderListenerEndpoint);
                pingServer.Bind(Settings.PingListenerEndpoint);

                logger.Log($"ElectionListenerEndpoint: {Settings.ElectionListenerEndpoint}");
                logger.Log($"LeaderListenerEndpoint: {Settings.LeaderListenerEndpoint}");
                logger.Log($"PingListenerEndpoint: {Settings.PingListenerEndpoint}");

                // Listen for new messages on the electionServer socket
                electionServer.ReceiveReady += (s, a) =>
                {
                    var msg = a.Socket.ReceiveFrameString();

                    logger.Log($"ELECTION MESSAGE: {msg}");
                    if (msg == Message.Election)
                    {
                        a.Socket.SendFrame(msg == Message.Election
                            ? Message.Ok
                            : Message.Fail);
                    }
                };

                // Listen for new messages on the leaderServer socket
                leaderServer.ReceiveReady += (s, a) =>
                {
                    var winnerMessage = a.Socket.ReceiveFrameString();
                    OnLeaderChanged(winnerMessage);
                    logger.Log($"NEW LEADER MESSAGE RECEIVED: {winnerMessage}");
                };

                // Listen for pings
                pingServer.ReceiveReady += (s, a) =>
                {
                    a.Socket.ReceiveFrameString();
                    a.Socket.SendFrame(Message.Ok);
                };

                logger.Log("-----------------------------------");

                poller.Run();
            }
        }

        protected virtual void OnLeaderChanged(string leaderId)
        {
            var newLeader = int.Parse(leaderId);
            LeaderChanged?.Invoke(this, newLeader);
        }
    }
}