using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    /// <summary>
    /// The interface for all systems in ECS.
    /// </summary>
    public interface ISystem
    {
        void Update(World world, double deltaTime, UpdateSettings updateSettings);
    }
}
