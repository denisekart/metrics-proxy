using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;

namespace MetricsProxy.Application.Application
{
    public class DefaultBackgroundServiceTracker : IBackgroundServiceTracker
    {
        private string _state = "Not initialized";

        public void Report(string state)
        {
            _state = state;
        }

        public string Query() => _state;
    }
}
