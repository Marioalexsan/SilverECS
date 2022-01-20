using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    internal struct ComponentSet
    {
        public List<object> Components;

        public List<Type> Types;

        public static ComponentSet From(object[] components)
        {
            if (components.Any(x => x is null))
            {
                throw new InvalidOperationException("From method requires non-null components");
            }

            return new ComponentSet()
            {
                Components = new List<object>(components),
                Types = new List<Type>(components.Select(x => x.GetType()))
            };
        }
    }
}
