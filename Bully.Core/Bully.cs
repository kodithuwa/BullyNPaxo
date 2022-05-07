﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Bully.Core.Logging;
using NetMQ;

namespace Bully.Core
{
    public class Bully
    {
        private readonly ILogger logger;
        private readonly CancellationTokenSource cts;
        private readonly LeaderElection leaderElection;
        private readonly MessageListener messageListener;
        private static readonly object Lockobj = new object();

        public event EventHandler<int> LeaderChanged;

        public Bully(ILogger logger)
        {
            this.logger = logger;

            cts = new CancellationTokenSource();

            var nodes = NodeFactory.CreateNodes();
            leaderElection = new LeaderElection(nodes, Lockobj, logger);
            messageListener = new MessageListener(logger);
            messageListener.LeaderChanged += OnMessageLeaderChanged;
        }

        public void Start()
        {
            logger.Log($"Starting message listener");

            Task.Factory.StartNew(
                () => messageListener.Listen(cts.Token),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            
            Thread.Sleep(2000);

            logger.Log($"Starting leader election");

            Task.Factory.StartNew(
                () => leaderElection.Run(cts.Token),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Stop()
        {
            cts.Cancel();
        }

        private void OnMessageLeaderChanged(object sender, int e)
        {
            OnLeaderChanged(e);
        }

        private void OnLeaderChanged(int e)
        {
            LeaderChanged?.Invoke(this, e);
        }
    }
}
