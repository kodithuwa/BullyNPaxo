using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bully.Core.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Bully.Core
{
    public class LeaderElection
    {
        public bool IsLeaderProcess;

        private readonly IList<Node> nodes;
        private readonly Node localNode;
        private readonly object lockObject;
        private readonly ILogger logger;

        public LeaderElection(IList<Node> nodes, object lockObject, ILogger logger)
        {
            this.nodes = nodes;
            this.lockObject = lockObject;
            this.logger = logger;

            localNode = this.nodes.First(c => c.IsLocal);
        }

        public void Run(CancellationToken cancellationToken)
        {
            InitLeaderElection();

            while (!cancellationToken.IsCancellationRequested)
            {
                SendPing(nodes);
                Thread.Sleep(Settings.PingInterval);
            }
        }

        private void InitLeaderElection()
        {
            try
            {
                logger.Log($"ELECT NEW LEADER INIT ({localNode.Id})");

                lock (lockObject)
                {
                    // Todo: check if there is a master already
                    ElectNewMaster(localNode, nodes);

                    if (IsLeaderProcess)
                    {
                        logger.Log($"I AM NEW LEADER: {localNode.Id}");
                        BroadcastVictory(nodes);
                        PaxoRun(nodes); 
                    }
                }

                logger.Log($"ELECT NEW LEADER END ({localNode.Id})");
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }
        }

        public void ElectNewMaster(Node thisNode, IList<Node> allNodes)
        {
            if (!thisNode.IsLocal)
            {
                throw new Exception("ElectNewMaster: thisNode is not local. Must initiate election message from my list of higher nodes.");
            }

            // If there are no higher nodes, I am the winner
            if (!thisNode.HigherNodes.Any())
            {
                IsLeaderProcess = true;
                thisNode.Role = NodeRole.Leader;
                return;
            }

            // If any higher thisNode responds to my election message with Ok
            // I stop bothering them since they outrank me
            if (thisNode.HigherNodes.OrderByDescending(i => i).Any(id => SendElection(allNodes[id])))
            {
                IsLeaderProcess = false;
                return;
            }

            IsLeaderProcess = true;
            thisNode.Role = NodeRole.Leader;

        }

        private bool SendElection(Node node)
        {
            if (node.IsLocal)
            {
                throw new Exception("SendElection: node is local. Cannot send election message to my self.");
            }

            string reply;

            logger.Log($"Sending election message to {node.ElectionSocket}");

            bool didAnswer;

            using (var client = new RequestSocket())
            {
                client.Connect(node.ElectionSocket);

                // Send election message to higher node
                client.TrySendFrame(TimeSpan.FromMilliseconds(1000), Message.Election);

                // Wait for reply OK/NOK or timeout
                didAnswer = client.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out reply);

                client.Close();
            }

            if (didAnswer)
            {
                return reply == Message.Ok;
            }

            return false;
        }

        private void BroadcastVictory(IList<Node> allNodes)
        {
            foreach (var node in allNodes)
            {
                logger.Log($"SENDING VICTORY MSG TO: {node.Id}");

                using (var client = new RequestSocket())
                {
                    client.Connect(node.LeaderSocket);
                    var couldSend = client.TrySendFrame(Settings.VictoryMessageTimeout, localNode.Id.ToString());
                    logger.Log($"COULD SEND: {couldSend}");
                    client.Close();
                }
            }
        }

        private void SendPing(IList<Node> allNodes)
        {
            var nodesDown = new List<Node>();
            foreach (var node in allNodes.Where(x => !x.IsLocal))
            {
                logger.Log($"PINGING: {node.PingSocket}");

                using (var client = new RequestSocket())
                {
                    client.Connect(node.PingSocket);
                    client.TrySendFrame(Settings.PingTimeout, localNode.Id.ToString());

                    string pingReply;
                    var replied = client.TryReceiveFrameString(Settings.PingTimeout, out pingReply);
                    client.Close();

                    logger.Log($"PING RESPONSE FROM: {node.PingSocket} {replied} {pingReply}");

                    if (!replied || pingReply != Message.Ok)
                    {
                        nodesDown.Add(node);
                    }
                }
            }

            if (nodesDown.Any())
            {
                InitLeaderElection();
            }
        }

        private void PaxoRun(IList<Node> nodes)
        {
            var paxo = new Paxos();
            var peersWithRoles = paxo.SetRoles(nodes);
            var proposers = peersWithRoles.Where(x => x.Role == NodeRole.Proposer);
            var numbers = paxo.GetValuesFromFile();
            var proposerResult = paxo.SetupProposer(proposers, numbers);

            //verify by acceptors
            var acceptors = peersWithRoles.Where(x => x.Role == NodeRole.Acceptor);
            var acceptorResult = paxo.VerifyByAcceptors(acceptors, proposerResult);

            //confirmed by learner
            var learnerList = paxo.FinalizedByLearner(proposers, acceptorResult);
            if (learnerList.Any())
            {
                foreach (var entry in learnerList)
                {
                    System.Console.WriteLine($"{entry.Key} - {entry.Value}");
                }
            }
        }
    }
}