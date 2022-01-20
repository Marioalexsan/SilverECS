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
    internal class ArchetypeManager : IEntityManager
    {
        private World world;

        private Archetype emptyArchetype = new Archetype(Array.Empty<Type>());

        private HashSet<Archetype> archetypes = new HashSet<Archetype>();

        private Dictionary<Type, HashSet<Archetype>> archetypeMap = new Dictionary<Type, HashSet<Archetype>>();

        private long nextEntityID = Entity.Null.ID + 1;

        private HashSet<Entity> entities = new HashSet<Entity>();

        private Dictionary<Entity, Archetype> entityToArchetype = new Dictionary<Entity, Archetype>();

        public int EntityCount => entities.Count;

        public ArchetypeManager(World world)
        {
            this.world = world;

            archetypes.Add(emptyArchetype);
        }

        /// <summary>
        /// Creates an empty entity and returns its ID.
        /// </summary>
        public Entity CreateEntity()
        {
            Entity entity = new Entity(world, GetEntityID());

            entities.Add(entity);

            emptyArchetype.AddEntity(entity);
            entityToArchetype[entity] = emptyArchetype;

            return entity;
        }

        /// <summary>
        /// Removes an entity and its components, if it exists.
        /// </summary>
        public bool DestroyEntity(Entity entityID)
        {
            if (!entities.Remove(entityID))
            {
                return false;
            }

            GetEntityArchetype(entityID).RemoveEntity(entityID);
            entityToArchetype.Remove(entityID);

            entityID.World = null;

            return false;
        }

        /// <summary>
        /// Returns true if the given ID corresponds to an entity, false otherwise.
        /// </summary>
        public bool HasEntity(Entity entityID)
        {
            return entities.Contains(entityID);
        }

        /// <summary>
        /// If an entity has an <see cref="EntityParent"/> component, the value of
        /// the parent is returned, otherwise <see cref="Entity.Null"/> is returned.
        /// </summary>
        public Entity GetParent(Entity entityID)
        {
            if (GetComponent(entityID, out EntityParent component))
            {
                return component.Parent;
            }
            else
            {
                return Entity.Null;
            }
        }

        /// <summary>
        /// Adds a component with the given value if the entity doesn't have one of that type already.
        /// Returns true if the addition succeeded, false otherwise.
        /// </summary>
        public bool AddComponent<T>(Entity entityID, in T component)
        {
            Archetype oldArchetype = GetEntityArchetype(entityID);

            if (oldArchetype.HasType(typeof(T)))
            {
                return false;
            }

            oldArchetype.ExtractEntity(entityID, out ComponentSet set);

            List<Type> newFilter = oldArchetype.Filter.ToList();

            newFilter.Add(typeof(T));
            set.Components.Add(component);
            set.Types.Add(typeof(T));

            Archetype newArchetype = GetOrCreateArchetype(newFilter);

            newArchetype.InjectEntity(entityID, set);
            entityToArchetype[entityID] = newArchetype;

            return true;
        }

        /// <summary>
        /// Removes the component of the given type if the entity has one.
        /// Returns true if the removal succeeded, false otherwise.
        /// </summary>
        public bool RemoveComponent<T>(Entity entityID)
        {
            Archetype oldArchetype = GetEntityArchetype(entityID);

            if (!oldArchetype.HasType(typeof(T)))
            {
                return false;
            }

            oldArchetype.ExtractEntity(entityID, out ComponentSet set);

            List<Type> newFilter = oldArchetype.Filter.ToList();

            newFilter.Remove(typeof(T));

            int index = set.Types.FindIndex(x => x == typeof(T));

            set.Components.RemoveAt(index);
            set.Types.RemoveAt(index);

            Archetype newArchetype = GetOrCreateArchetype(newFilter);

            newArchetype.InjectEntity(entityID, set);
            entityToArchetype[entityID] = newArchetype;

            return true;
        }

        /// <summary>
        /// If the entity has a component of the given type, returns its value in the out parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        public bool GetComponent<T>(Entity entityID, out T component)
        {
            if (!HasEntity(entityID))
            {
                component = default;
                return false;
            }
            else
            {
                Archetype archetype = GetEntityArchetype(entityID);

                if (archetype != emptyArchetype)
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
        public bool HasComponent<T>(Entity entityID)
        {
            Archetype archetype;

            return
                HasEntity(entityID) &&
                (archetype = GetEntityArchetype(entityID)) != null &&
                archetype.HasType(typeof(T));
        }

        /// <summary>
        /// If the entity has a component of the given type, sets its value to the parameter.
        /// Returns true if the entity does have a component of the given type, false otherwise.
        /// </summary>
        public bool SetComponent<T>(Entity entityID, in T component)
        {
            if (!HasEntity(entityID))
            {
                return false;
            }
            else
            {
                Archetype archetype = GetEntityArchetype(entityID);

                if (archetype != emptyArchetype)
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
        public IEnumerable<Entity> QueryEntities<A>()
        {
            return QueryEntities(typeof(A));
        }

        /// <summary>
        /// Returns all entities that have components of type A and B.
        /// Returned entities can have more components, but they will at least match A and B.
        /// </summary>
        public IEnumerable<Entity> QueryEntities<A, B>()
        {
            return QueryEntities(typeof(A), typeof(B));
        }

        /// <summary>
        /// Returns all entities that have components of type A, B and C.
        /// Returned entities can have more components, but they will at least match A, B and C.
        /// </summary>
        public IEnumerable<Entity> QueryEntities<A, B, C>()
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C));
        }

        /// <summary>
        /// Returns all entities that have components of type A, B, C and D.
        /// Returned entities can have more components, but they will at least match A, B, C and D.
        /// </summary>
        public IEnumerable<Entity> QueryEntities<A, B, C, D>()
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C), typeof(D));
        }

        /// <summary>
        /// Returns all entities that have components of the given types.
        /// Returned entities can have more components, but they will at least match the type list.
        /// </summary>
        public IEnumerable<Entity> QueryEntities(params Type[] types)
        {
            if (types.Distinct().Count() != types.Length)
            {
                throw new InvalidOperationException("Duplicate types are not allowed!");
            }

            List<Entity> matches = new List<Entity>();

            foreach (Archetype archetype in GetCompatibleArchetypes(types))
            {
                matches.AddRange(archetype.GetEntities());
            }

            return matches;
        }

        private long GetEntityID()
        {
            return nextEntityID++;
        }

        private Archetype GetEntityArchetype(Entity entityID)
        {
            return entityToArchetype[entityID];
        }

        private Archetype GetOrCreateArchetype(List<Type> types)
        {
            if (types.Count == 0)
            {
                return emptyArchetype;
            }

            foreach (Archetype archetype in archetypes)
            {
                IReadOnlyList<Type> filter = archetype.Filter;

                if (filter.Count == types.Count && new HashSet<Type>(filter.AsEnumerable()).SetEquals(types))
                {
                    return archetype;
                }
            }

            Archetype newArchetype = new Archetype(types);
            archetypes.Add(newArchetype);

            foreach (Type type in newArchetype.Filter.ToList())
            {
                if (!archetypeMap.TryGetValue(type, out HashSet<Archetype> set))
                {
                    set = archetypeMap[type] = new HashSet<Archetype>();
                }

                set.Add(newArchetype);
            }

            return newArchetype;
        }

        private IEnumerable<Archetype> GetCompatibleArchetypes(IEnumerable<Type> types)
        {
            if (!types.Any())
            {
                return new Archetype[]
                {
                    emptyArchetype
                };
            }

            HashSet<Archetype> pool = null;

            foreach (Type type in types)
            {
                if (archetypeMap.TryGetValue(type, out HashSet<Archetype> restriction))
                {
                    if (pool != null)
                    {
                        pool.IntersectWith(restriction);
                    }
                    else
                    {
                        pool = new HashSet<Archetype>(restriction);
                    }
                }
                else
                {
                    return Array.Empty<Archetype>();
                }
            }

            return pool;
        }
    }
}
