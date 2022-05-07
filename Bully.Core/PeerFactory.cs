using System;
using System.Collections.Generic;
using System.Linq;

namespace Bully.Core
{
    public static class NodeFactory
    {
        private const char ClusterMemberDelimiter = ',';

        public static List<Node> CreateNodes()
        {
            var electionSockets = Settings.ElectionEndpoints.Split(ClusterMemberDelimiter).ToList();
            var leaderSockets = Settings.LeaderEndpoints.Split(ClusterMemberDelimiter).ToList();
            var pingSockets = Settings.PingEndpoints.Split(ClusterMemberDelimiter).ToList();
            var acceptorSockets = Settings.AcceptorEndpoints.Split(ClusterMemberDelimiter).ToList();
            var proposerSockets = Settings.ProposerEndpoints.Split(ClusterMemberDelimiter).ToList();

            var thisCandidateId = int.Parse(Settings.NodeId);
            var candidates = new List<Node>();

            var index = 0;
            foreach (var electionUri in electionSockets)
            {
                var rnd = new Random();
                var rndValue = rnd.Next();
                var nodeId = DateTime.Now.Millisecond + rndValue;
                candidates.Add(new Node(nodeId,
                                             electionUri,
                                             leaderSockets[index],
                                             pingSockets[index],
                                             index == thisCandidateId,
                                             GetMoreAuthoritativeCandidateIds(nodeId, electionSockets)
                                             .Where(i => i >= 0).ToList(),
                                             acceptorSockets[index],
                                             proposerSockets[index]));
                index++;
            }

            return candidates;
        }

        private static IEnumerable<int> GetMoreAuthoritativeCandidateIds(int index, List<string> candidateUris)
        {
            return candidateUris.Select(n => {
                var i = Array.IndexOf<string>(candidateUris.ToArray(), n);
                if (i > index)
                {
                    return i;
                }

                return -1;
            });
        }
    }
}