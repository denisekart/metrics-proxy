using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Contracts
{
    public interface IBackgroundServiceTracker
    {
        void Report(string state);
        string Query();
    }
}
