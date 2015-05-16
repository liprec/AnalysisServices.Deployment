using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices.DeploymentEngine;

namespace AnalysisServices.Deployment
{
    class DeploymentTracer : IDeploymentTrace
    {
        public void Trace(string message)
        {
            Program.TraceProgress(message);
        }
        public void ProcessComplete(bool success, System.Exception ex)
        {
            if (!success)
            {
                Program.TraceProgress(ex.Message);
            }
        }
    }
}
