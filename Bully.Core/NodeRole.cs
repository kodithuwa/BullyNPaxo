using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bully.Core
{
   public enum NodeRole
    {
        None,
        Leader,
        Proposer,
        Acceptor,
        Learner
    }
}
