using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsProxy.Application.Contracts
{
    /// <summary>
    /// Used to track the background service state
    /// </summary>
    public interface IBackgroundServiceTracker
    {
        /// <summary>
        /// Set the current state
        /// </summary>
        /// <param name="state">The state to set</param>
        void Report(string state);
        /// <summary>
        /// Query the current state
        /// </summary>
        string Query();
    }
}
