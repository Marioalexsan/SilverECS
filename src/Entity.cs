using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    [DebuggerDisplay("ID = {ID}")]
    public class Entity : IEquatable<Entity>
    {
        public static readonly Entity Null = new Entity(null, 0);

        public World World { get; internal set; }

        public long ID { get; }

        public bool IsDestroyed => World == null;

        internal Entity(World world, long id)
        {
            World = world;
            ID = id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity);
        }

        public bool Equals(Entity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return this.ID == other.ID;
        }

        public static bool operator ==(Entity one, Entity two)
        {
            if (ReferenceEquals(one, two))
            {
                return true;
            }

            if (one is null || two is null)
            {
                return false;
            }

            return one.Equals(two);
        }

        public static bool operator !=(Entity one, Entity two)
        {
            return !(one == two);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID.ToString();
        }

        /// <summary>
        /// Adds a component to this entity.
        /// </summary>
        public bool Add<Component>(Component component)
        {
            return World.AddComponent(this, component);
        }

        /// <summary>
        /// Calls <see cref="ArchetypeManager.GetComponent{T}(Entity, out T)"/> on the given entity.
        /// </summary>
        public bool Get<Component>(out Component component)
        {
            return World.GetComponent(this, out component);
        }

        /// <summary>
        /// Calls <see cref="ArchetypeManager.HasComponent{T}(Entity)"/> on the given entity.
        /// </summary>
        public bool Has<Component>()
        {
            return World.HasComponent<Component>(this);
        }

        /// <summary>
        /// Calls <see cref="ArchetypeManager.SetComponent{T}(Entity, in T)"/> on the given entity.
        /// </summary>
        public bool Set<Component>(Component component)
        {
            return World.SetComponent(this, component);
        }

        /// <summary>
        /// Calls <see cref="ArchetypeManager.RemoveComponent{T}(Entity)"/> on the given entity.
        /// </summary>
        public bool Remove<Component>()
        {
            return World.RemoveComponent<Component>(this);
        }
    }
}
