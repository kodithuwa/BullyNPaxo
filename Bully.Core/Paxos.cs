using System;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Bully.Core.Logging;
using NetMQ;
using System.Collections.Generic;
using System.IO;
using NetMQ.Sockets;

namespace Bully.Core
{
    public class Paxos
    {
        public IEnumerable<Node> SetRoles(IList<Node> nodes)
        {
            var nodesExceptLeader = nodes.Where(x => x.Role != NodeRole.Leader);
            nodesExceptLeader.ElementAt(0).Role = NodeRole.Acceptor;
            nodesExceptLeader.ElementAt(1).Role = NodeRole.Acceptor;
            nodesExceptLeader.ElementAt(2).Role = NodeRole.Learner;
            nodesExceptLeader.ElementAt(3).Role = NodeRole.Proposer;

            return nodesExceptLeader;
        }

        public IEnumerable<int> GetValuesFromFile()
        {
            string[] lines = File.ReadAllLines("numbers.txt");
            var numbers = new List<int>();
            lines.ToList().ForEach(x =>
            {
                var num = Convert.ToInt32(x.Trim());
                numbers.Add(num);
            });
            return numbers;
        }

        public Dictionary<string,string> SetupProposer(IEnumerable<Node> proposers, IEnumerable<int> valueList)
        {
            var valueDic = this.CheckNumbers(proposers, valueList, NodeRole.Proposer);
            return valueDic;
        }

        public Dictionary<string, string> VerifyByAcceptors(IEnumerable<Node> acceptors, Dictionary<string,string> proposerRespond)
        {
            var nonPrimeList = proposerRespond.Where(x => x.Value.Contains("is not a prime number"));
            if (nonPrimeList.Any())
            {
                var keys = nonPrimeList.Select(x => x.Key).ToList();
                var numbers = new List<int>();
                keys.ForEach(x =>
                {
                    var number = Convert.ToInt32( x.Split("-")[0]);
                    numbers.Add(number);
                });

                var result = this.CheckNumbers(acceptors,numbers , NodeRole.Acceptor);
                if (result.Any(x => x.Value.Contains("is a prime number")))
                {
                    foreach(var item in proposerRespond)
                    {
                        var number = item.Key.Split("-")[0];
                        var primeKeys = proposerRespond.Keys.Where(x => x.Contains(number));
                        if(primeKeys.Count() == acceptors.Count())
                        {
                            proposerRespond[item.Key] = $"{number} is a prime number";
                        }
                        
                    }
                }

            }

            return proposerRespond;
        }

        public Dictionary<string, string> FinalizedByLearner(IEnumerable<Node> proposers, Dictionary<string,string> acceptedResult)
        {
            var result = new Dictionary<string, string>();
            var noOfProposers = proposers.Count();
            var keys = acceptedResult.Keys.Distinct().ToList();
            var numbers = new List<int>();
            keys.ForEach(x =>
            {
                var number = Convert.ToInt32(x.Split("-")[0]);
                numbers.Add(number);
            });

            foreach (var number in numbers.Distinct())
            {
                var isPrime = acceptedResult.Values.Where(x => x.Contains(number.ToString())).All(x => x.Contains("is a prime number"));
                if(isPrime)
                {
                    result.Add(number.ToString(), $"{number} is a prime number.");
                }
                else
                {
                    result.Add(number.ToString(), $"{number} is not a prime number.");
                }
            }

            return result;
        }


        public Dictionary<string, string> CheckNumbers(IEnumerable<Node> proposersOrAcceptors, IEnumerable<int> valueList, NodeRole role)
        {
            var values = valueList.ToList();
            var maxValue = values.Max();
            var count = values.Count();
            var range = maxValue / count;
            int rangeStart = 2;
            int rangeEnd = range;
            var valueDic = new Dictionary<string, string>();

            // No of values
            for (int i = 0; i < count; i++)
            {
                var proposerIndex = 0;
                // each number distribute among all proporsers
                foreach (var proposer in proposersOrAcceptors)
                {
                    var nodeSocket = role == NodeRole.Acceptor ? proposer.AcceptorSocket : proposer.ProposerSocket;

                    using (var proposerResponse = new ResponseSocket(nodeSocket))
                    using (var proposerRequest = new RequestSocket(nodeSocket))
                    {
                        // pass to proposer from the leader
                        proposerRequest.SendFrame(values[i].ToString());
                        Console.WriteLine("From Leader: {0}", values[i].ToString());

                        // Processing by Proposer
                        Console.WriteLine($"Processing by {$"{proposer.PingSocket}/{role.ToString()}"}");
                        var valueToProposer = Convert.ToInt32(proposerResponse.ReceiveFrameString());
                        rangeStart = proposerIndex == 0 ? rangeStart : rangeStart + range;
                        rangeEnd = proposerIndex == 0 ? rangeEnd : rangeEnd + range;
                        var reply = this.CheckNumber(rangeStart, rangeEnd, valueToProposer);
                        Console.WriteLine(reply);
                        var uniqueId = $"{valueToProposer.ToString()}-{proposer.Id.ToString()}";
                        valueDic.Add(uniqueId, reply);
                    }
                }
            }

            return valueDic;
        }


        public string CheckNumber(int rangeStart, int rangeEnd, int value)
        {
            for(int i = rangeStart; i <= rangeEnd; i++)
            {
                if(rangeEnd == value)
                {
                    continue;
                }

                if(value % i == 0)
                {
                    return $"{value} is not a prime number, divided by {i}";
                }
            }

            return $"{value} is a prime number.";
        }


    }
}
