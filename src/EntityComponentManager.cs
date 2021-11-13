using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SilverECS
{
    [DebuggerDisplay("Entities = {EntityCount}, Factories = {_factories.Count}")]
    internal class EntityComponentManager : IEntityComponentManager
    {
        private World _world;

        private Dictionary<int, Action<EntityID, World>> _factories = new Dictionary<int, Action<EntityID, World>>();

        private EntityArchetype _emptyArchetype = new EntityArchetype(new Type[0]);

        private HashSet<EntityArchetype> _archetypes = new HashSet<EntityArchetype>();

        private Dictionary<Type, HashSet<EntityArchetype>> _archetypeMap = new Dictionary<Type, HashSet<EntityArchetype>>();

        private int _nextEntityID = EntityID.Null.ID + 1;

        private HashSet<EntityID> _entities = new HashSet<EntityID>();

        private Dictionary<EntityID, EntityArchetype> _entityToArchetype = new Dictionary<EntityID, EntityArchetype>();

        public int EntityCount => _entities.Count;

        public EntityComponentManager(World world)
        {
            _world = world;

            _archetypes.Add(_emptyArchetype);
        }

        /// <summary>
        /// Creates an empty entity and returns its ID.
        /// </summary>
        public EntityID CreateEntity()
        {
            EntityID entity = new EntityID(GetEntityID());

            _entities.Add(entity);

            _emptyArchetype.AddEntity(entity);
            _entityToArchetype[entity] = _emptyArchetype;

            return entity;
        }

        /// <summary>
        /// Creates an entity using its entity type's factory, and returns its ID.
        /// </summary>
        public EntityID CreateEntity(int entityType)
        {
            EntityID entity = CreateEntity();

            if (_factories.ContainsKey(entityType))
            {
                _factories[entityType].Invoke(entity, _world);
            }

            return entity;
        }

        /// <summary>
        /// Removes an entity and its components, if it exists.
        /// </summary>
        public bool DestroyEntity(EntityID entityID)
        {
            if (!_entities.Remove(entityID))
            {
                return false;
            }

            GetEntityArchetype(entityID).RemoveEntity(entityID);
            _entityToArchetype.Remove(entityID);

            return false;
        }

        /// <summary>
        /// Returns true if the given ID corresponds to an entity, false otherwise.
        /// </summary>
        public bool EntityExists(EntityID entityID)
        {
            return _entities.Contains(entityID);
        }

        /// <summary>
        /// If an entity has an <see cref="EntityParent"/> component, the value of
        /// the parent is returned, otherwise <see cref="EntityID.Null"/> is returned.
        /// </summary>
        public EntityID GetParent(EntityID entityID)
        {
            if (TryGetComponent(entityID, out EntityParent component))
            {
                return component.Parent;
            }
            else
            {
                return EntityID.Null;
            }
        }

        /// <summary>
        /// Sets a factory for an entity type.
        /// Afterwards, you can specify that entity type in <see cref="CreateEntity(int)"/>
        /// to create an entity using that factory.
        /// </summary>
        public void SetFactory(int entityType, Action<EntityID, World> factory)
        {
            if (factory != null)
            {
                _factories[entityType] = factory;
            }
            else
            {
                _factories.Remove(entityType);
            }
        }

        /// <summary>
        /// Checks if an entity type has a factory assigned.
        /// </summary>
        public bool HasFactory(int entityType)
        {
            return _factories.ContainsKey(entityType);
        }

        /// <summary>
        /// Adds a default-initialized component if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        public bool AddComponent<T>(EntityID entityID)
            where T : struct
        {
            EntityArchetype oldArchetype = GetEntityArchetype(entityID);

            if (oldArchetype.HasType(typeof(T)))
            {
                return false;
            }

            oldArchetype.ExtractAndRemoveEntity(entityID, out List<object> components);

            List<Type> newFilter = oldArchetype.Filter.ToList();

            newFilter.Add(typeof(T));
            components.Add(default(T));

            EntityArchetype newArchetype = GetOrCreateArchetype(newFilter);

            newArchetype.AddAndPopulateEntity(entityID, components);
            _entityToArchetype[entityID] = newArchetype;

            return true;
        }

        /// <summary>
        /// Adds a component with the given value if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        public bool AddComponent<T>(EntityID entityID, in T component)
            where T : struct
        {
            if (AddComponent<T>(entityID))
            {
                SetComponent(entityID, component);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the component of the given type if the entity has one.
        /// Returns true if the removal succeeded, false otherwise.
        /// </summary>
        public bool RemoveComponent<T>(EntityID entityID)
            where T : struct
        {
            EntityArchetype oldArchetype = GetEntityArchetype(entityID);

            if (!oldArchetype.HasType(typeof(T)))
            {
                return false;
            }

            oldArchetype.ExtractAndRemoveEntity(entityID, out List<object> components);

            List<Type> newFilter = oldArchetype.Filter.ToList();

            newFilter.Remove(typeof(T));
            components.Remove(components.First(x => x.GetType() == typeof(T)));

            EntityArchetype newArchetype = GetOrCreateArchetype(newFilter);

            newArchetype.AddAndPopulateEntity(entityID, components);
            _entityToArchetype[entityID] = newArchetype;

            return true;
        }

        /// <summary>
        /// If the entity has a component of the given type, returns its value in the out parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        public bool TryGetComponent<T>(EntityID entityID, out T component)
            where T : struct
        {
            if (!EntityExists(entityID))
            {
                component = default;
                return false;
            }
            else
            {
                EntityArchetype archetype = GetEntityArchetype(entityID);

                if (archetype != _emptyArchetype)
                {
                    return archetype.GetComponent(entityID, out component);
                }
                else
                {
                    component = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the entity has the given component, false otherwise.
        /// </summary>
        public bool HasComponent<T>(EntityID entityID)
            where T : struct
        {
            EntityArchetype archetype;

            return
                EntityExists(entityID) &&
                (archetype = GetEntityArchetype(entityID)) != null &&
                archetype.HasType(typeof(T));
        }

        /// <summary>
        /// If the entity has a component of the given type, sets its value to the parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        public bool SetComponent<T>(EntityID entityID, in T component)
            where T : struct
        {
            if (!EntityExists(entityID))
            {
                return false;
            }
            else
            {
                EntityArchetype archetype = GetEntityArchetype(entityID);

                if (archetype != _emptyArchetype)
                {
                    return archetype.SetComponent(entityID, in component);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns all entities that have a component of type A.
        /// Returned entities can have more components, but they will at least match A.
        /// </summary>
        public IEnumerable<EntityID> QueryEntities<A>()
            where A : struct
        {
            return QueryEntities(typeof(A));
        }

        /// <summary>
        /// Returns all entities that have components of type A and B.
        /// Returned entities can have more components, but they will at least match A and B.
        /// </summary>
        public IEnumerable<EntityID> QueryEntities<A, B>()
            where A : struct
            where B : struct
        {
            return QueryEntities(typeof(A), typeof(B));
        }

        /// <summary>
        /// Returns all entities that have components of type A, B and C.
        /// Returned entities can have more components, but they will at least match A, B and C.
        /// </summary>
        public IEnumerable<EntityID> QueryEntities<A, B, C>()
            where A : struct
            where B : struct
            where C : struct
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C));
        }

        /// <summary>
        /// Returns all entities that have components of type A, B, C and D.
        /// Returned entities can have more components, but they will at least match A, B, C and D.
        /// </summary>
        public IEnumerable<EntityID> QueryEntities<A, B, C, D>()
            where A : struct
            where B : struct
            where C : struct
            where D : struct
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C), typeof(D));
        }

        /// <summary>
        /// Returns all entities that have components of the given types.
        /// Returned entities can have more components, but they will at least match the type list.
        /// </summary>
        public IEnumerable<EntityID> QueryEntities(params Type[] types)
        {
            if (types.Distinct().Count() != types.Length)
            {
                throw new InvalidOperationException("Duplicate types are not allowed!");
            }

            if (types.Any(x => !x.IsValueType || x.IsPrimitive))
            {
                throw new InvalidOperationException("one or more types are not component types!");
            }

            List<EntityID> matches = new List<EntityID>();

            foreach (EntityArchetype archetype in GetCompatibleArchetypes(types))
            {
                matches.AddRange(archetype.GetEntities());
            }

            return matches;
        }

        private int GetEntityID()
        {
            return _nextEntityID++;
        }

        private EntityArchetype GetEntityArchetype(EntityID entityID)
        {
            return _entityToArchetype[entityID];
        }

        private EntityArchetype GetOrCreateArchetype(List<Type> types)
        {
            if (types.Count == 0)
            {
                return _emptyArchetype;
            }

            foreach (EntityArchetype archetype in _archetypes)
            {
                IReadOnlyList<Type> filter = archetype.Filter;

                if (filter.Count == types.Count && new HashSet<Type>(filter.AsEnumerable()).SetEquals(types))
                {
                    return archetype;
                }
            }

            EntityArchetype newArchetype = new EntityArchetype(types);
            _archetypes.Add(newArchetype);

            foreach (Type type in newArchetype.Filter.ToList())
            {
                if (!_archetypeMap.TryGetValue(type, out HashSet<EntityArchetype> set))
                {
                    set = _archetypeMap[type] = new HashSet<EntityArchetype>();
                }

                set.Add(newArchetype);
            }

            return newArchetype;
        }

        private IEnumerable<EntityArchetype> GetCompatibleArchetypes(IEnumerable<Type> types)
        {
            if (types.Count() == 0)
            {
                return new EntityArchetype[]
                {
                    _emptyArchetype
                };
            }

            HashSet<EntityArchetype> pool = null;

            foreach (Type type in types)
            {
                if (_archetypeMap.TryGetValue(type, out HashSet<EntityArchetype> restriction))
                {
                    if (pool != null)
                    {
                        pool.IntersectWith(restriction);
                    }
                    else
                    {
                        pool = new HashSet<EntityArchetype>(restriction);
                    }
                }
                else
                {
                    return Array.Empty<EntityArchetype>();
                }
            }

            return pool;
        }
    }
}
