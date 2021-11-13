using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    // Shamelessly stolen from Encompass :(
    [DebuggerDisplay("ID = {ID}")]
    public struct EntityID : IEquatable<EntityID>
    {
        public static readonly EntityID Null = new EntityID(0);

        public readonly int ID;

        internal EntityID(int id)
        {
            ID = id;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityID entity && Equals(entity);
        }

        public bool Equals(EntityID other)
        {
            return other.ID == ID;
        }

        public static bool operator ==(EntityID one, EntityID two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(EntityID one, EntityID two)
        {
            return !one.Equals(two);
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
