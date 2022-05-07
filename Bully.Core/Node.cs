using System.Collections.Generic;

namespace Bully.Core
{
    public class Node
    {
        public Node(int id, string electionSocket, string leaderSocket, string pingSocket, bool isLocal, List<int> higherNodes, string acceptorSocket, string proposerSocket)
        {
            Id = id;
            ElectionSocket = electionSocket;
            LeaderSocket = leaderSocket;
            PingSocket = pingSocket;
            IsLocal = isLocal;
            HigherNodes = higherNodes;
            AcceptorSocket = acceptorSocket;
            ProposerSocket = proposerSocket;
        }

        public int Id { get; set; }

        public string ElectionSocket { get; set; }

        public string LeaderSocket { get; set; }

        public string PingSocket { get; set; }

        public string ProposerSocket { get; set; }

        public string AcceptorSocket { get; set; }

        public bool IsLocal { get; set; }

        public List<int> HigherNodes { get; set; }

        public NodeRole Role { get; set; }
    }
}