using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SilverECS
{
    public interface IEntityComponentManager
    {
        /// <summary>
        /// Gets the current entity count.
        /// </summary>
        int EntityCount { get; }

        /// <summary>
        /// Adds a default-initialized component if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        bool AddComponent<T>(EntityID entityID)
            where T : struct;

        /// <summary>
        /// Adds a component with the given value if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        bool AddComponent<T>(EntityID entityID, in T component)
            where T : struct;

        /// <summary>
        /// Creates an empty entity and returns its ID.
        /// </summary>
        EntityID CreateEntity();

        /// <summary>
        /// Creates an entity using its entity type's factory, and returns its ID.
        /// </summary>
        EntityID CreateEntity(int entityType);

        /// <summary>
        /// Removes an entity and its components, if it exists.
        /// </summary>
        bool DestroyEntity(EntityID entityID);

        /// <summary>
        /// Returns true if the given ID corresponds to an entity, false otherwise.
        /// </summary>
        bool EntityExists(EntityID entityID);

        /// <summary>
        /// If the entity has a component of the given type, returns its value in the out parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        bool TryGetComponent<T>(EntityID entityID, out T component)
            where T : struct;

        /// <summary>
        /// Returns true if the entity has the given component, false otherwise.
        /// </summary>
        bool HasComponent<T>(EntityID entityID)
            where T : struct;

        /// <summary>
        /// If an entity has an <see cref="EntityParent"/> component, the value of
        /// the parent is returned, otherwise <see cref="EntityID.Null"/> is returned.
        /// </summary>
        EntityID GetParent(EntityID entityID);

        /// <summary>
        /// Checks if an entity type has a factory assigned.
        /// </summary>
        bool HasFactory(int entityType);

        /// <summary>
        /// Returns all entities that have a component of type A.
        /// Returned entities can have more components, but they will at least match A.
        /// </summary>
        IEnumerable<EntityID> QueryEntities<A>()
            where A : struct;

        /// <summary>
        /// Returns all entities that have components of type A and B.
        /// Returned entities can have more components, but they will at least match A and B.
        /// </summary>
        IEnumerable<EntityID> QueryEntities<A, B>()
            where A : struct
            where B : struct;

        /// <summary>
        /// Returns all entities that have components of type A, B and C.
        /// Returned entities can have more components, but they will at least match A, B and C.
        /// </summary>
        IEnumerable<EntityID> QueryEntities<A, B, C>()
            where A : struct
            where B : struct
            where C : struct;

        /// <summary>
        /// Returns all entities that have components of type A, B, C and D.
        /// Returned entities can have more components, but they will at least match A, B, C and D.
        /// </summary>
        IEnumerable<EntityID> QueryEntities<A, B, C, D>()
            where A : struct
            where B : struct
            where C : struct
            where D : struct;

        /// <summary>
        /// Returns all entities that have components of the given types.
        /// Returned entities can have more components, but they will at least match the type list.
        /// </summary>
        IEnumerable<EntityID> QueryEntities(params Type[] types);

        /// <summary>
        /// Removes the component of the given type if the entity has one.
        /// Returns true if the removal succeeded, false otherwise.
        /// </summary>
        bool RemoveComponent<T>(EntityID entityID)
            where T : struct;

        /// <summary>
        /// If the entity has a component of the given type, sets its value to the parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        bool SetComponent<T>(EntityID entityID, in T component)
            where T : struct;

        /// <summary>
        /// Sets a factory for an entity type.
        /// Afterwards, you can specify that entity type in <see cref="CreateEntity(int)"/>
        /// to create an entity using that factory.
        /// </summary>
        void SetFactory(int entityType, Action<EntityID, World> factory);
    }
}