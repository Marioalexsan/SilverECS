using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    public static class EntityIDExtension
    {
        /// <summary>
        /// Calls <see cref="EntityComponentManager.AddComponent{T}(EntityID, in T)"/> on the given entity.
        /// </summary>
        public static bool AddComponent<T>(this EntityID entityID, World world, T component)
            where T : struct
        {
            return world.AddComponent(entityID, component);
        }

        /// <summary>
        /// Calls <see cref="EntityComponentManager.TryGetComponent{T}(EntityID, out T)"/> on the given entity.
        /// </summary>
        public static bool TryGetComponent<T>(this EntityID entityID, World world, out T component)
            where T : struct
        {
            return world.TryGetComponent(entityID, out component);
        }

        /// <summary>
        /// Calls <see cref="EntityComponentManager.HasComponent{T}(EntityID)"/> on the given entity.
        /// </summary>
        public static bool HasComponent<T>(this EntityID entityID, World world)
            where T : struct
        {
            return world.HasComponent<T>(entityID);
        }

        /// <summary>
        /// Calls <see cref="EntityComponentManager.SetComponent{T}(EntityID, in T)"/> on the given entity.
        /// </summary>
        public static bool SetComponent<T>(this EntityID entityID, World world, T component)
            where T : struct
        {
            return world.SetComponent(entityID, component);
        }

        /// <summary>
        /// Calls <see cref="EntityComponentManager.RemoveComponent{T}(EntityID)"/> on the given entity.
        /// </summary>
        public static bool RemoveComponent<T>(this EntityID entityID, World world)
            where T : struct
        {
            return world.RemoveComponent<T>(entityID);
        }
    }
}
