using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SilverECS
{
    /// <summary>
    /// A builtin component which stores the parent of an entity (if any).
    /// </summary>
    [DebuggerDisplay("Parent = {Parent}")]
    public struct EntityParent
    {
        public EntityID Parent;
    }
}
