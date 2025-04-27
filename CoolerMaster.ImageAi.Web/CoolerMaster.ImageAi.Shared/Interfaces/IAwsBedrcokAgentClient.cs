using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Shared.Interfaces
{
    public interface IAwsBedrcokAgentClient
    {
        Task<string> InvokeAgentAsync(string inputText);
    }
}
