using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SilverECS
{
    public interface IEntityManager
    {
        /// <summary>
        /// Gets the current entity count.
        /// </summary>
        int EntityCount { get; }

        /// <summary>
        /// Creates an empty entity and returns its ID.
        /// </summary>
        Entity CreateEntity();

        /// <summary>
        /// Removes an entity and its components, if it exists.
        /// </summary>
        bool DestroyEntity(Entity entityID);

        /// <summary>
        /// Returns true if the given ID corresponds to an entity, false otherwise.
        /// </summary>
        bool HasEntity(Entity entityID);

        /// <summary>
        /// If an entity has an <see cref="EntityParent"/> component, the value of
        /// the parent is returned, otherwise <see cref="Entity.Null"/> is returned.
        /// </summary>
        Entity GetParent(Entity entityID);

        /// <summary>
        /// Adds a component with the given value if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        bool AddComponent<Component>(Entity entityID, in Component component);

        /// <summary>
        /// Removes the component of the given type if the entity has one.
        /// Returns true if the removal succeeded, false otherwise.
        /// </summary>
        bool RemoveComponent<Component>(Entity entityID);

        /// <summary>
        /// If the entity has a component of the given type, returns its value in the out parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        bool GetComponent<Component>(Entity entityID, out Component component);

        /// <summary>
        /// If the entity has a component of the given type, sets its value to the parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        bool SetComponent<Component>(Entity entityID, in Component component);

        /// <summary>
        /// Returns true if the entity has the given component, false otherwise.
        /// </summary>
        bool HasComponent<Component>(Entity entityID);

        /// <summary>
        /// Returns all entities that have a component of type A.
        /// Returned entities can have more components, but they will at least match A.
        /// </summary>
        IEnumerable<Entity> QueryEntities<A>();

        /// <summary>
        /// Returns all entities that have components of type A and B.
        /// Returned entities can have more components, but they will at least match A and B.
        /// </summary>
        IEnumerable<Entity> QueryEntities<A, B>();

        /// <summary>
        /// Returns all entities that have components of type A, B and C.
        /// Returned entities can have more components, but they will at least match A, B and C.
        /// </summary>
        IEnumerable<Entity> QueryEntities<A, B, C>();

        /// <summary>
        /// Returns all entities that have components of type A, B, C and D.
        /// Returned entities can have more components, but they will at least match A, B, C and D.
        /// </summary>
        IEnumerable<Entity> QueryEntities<A, B, C, D>();

        /// <summary>
        /// Returns all entities that have components of the given types.
        /// Returned entities can have more components, but they will at least match the type list.
        /// </summary>
        IEnumerable<Entity> QueryEntities(params Type[] types);
    }
}