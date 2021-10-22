using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    public interface ISystem
    {
        void Update(World world, double gameTime, double realTime);
    }
}
