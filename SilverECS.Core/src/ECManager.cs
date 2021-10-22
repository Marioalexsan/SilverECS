using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    public class ECManager
    {
        private EntityArchetype _emptyArchetype = new EntityArchetype(new Type[0]);

        private HashSet<EntityArchetype> _archetypes = new HashSet<EntityArchetype>();

        private Dictionary<Type, HashSet<EntityArchetype>> _archetypeMap = new Dictionary<Type, HashSet<EntityArchetype>>();

        private int _nextEntityID = EntityID.Null.ID + 1;

        private HashSet<EntityID> _entities = new HashSet<EntityID>();

        private Dictionary<EntityID, EntityArchetype> _entityArchetypes = new Dictionary<EntityID, EntityArchetype>();

        public int EntityCount => _entities.Count;

        public ECManager()
        {
            _archetypes.Add(_emptyArchetype);
        }

        public EntityID CreateEntity()
        {
            EntityID entity = new EntityID(GetEntityID());

            _entities.Add(entity);

            _emptyArchetype.AddEntity(entity);
            _entityArchetypes[entity] = _emptyArchetype;

            return entity;
        }

        public bool DestroyEntity(EntityID entityID)
        {
            if (!_entities.Remove(entityID))
            {
                return false;
            }

            GetEntityArchetype(entityID).RemoveEntity(entityID);
            _entityArchetypes.Remove(entityID);

            return false;
        }

        public bool EntityExists(EntityID entityID)
        {
            return _entities.Contains(entityID);
        }

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
            _entityArchetypes[entityID] = newArchetype;

            return true;
        }

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
            _entityArchetypes[entityID] = newArchetype;

            return true;
        }

        public bool GetComponent<T>(EntityID entityID, out T component)
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

        public bool AddAndSetComponent<T>(EntityID entityID, in T component)
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

        public IEnumerable<EntityID> QueryEntities<A>()
            where A : struct
        {
            return QueryEntities(typeof(A));
        }

        public IEnumerable<EntityID> QueryEntities<A, B>()
            where A : struct
            where B : struct
        {
            return QueryEntities(typeof(A), typeof(B));
        }

        public IEnumerable<EntityID> QueryEntities<A, B, C>()
            where A : struct
            where B : struct
            where C : struct
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C));
        }

        public IEnumerable<EntityID> QueryEntities<A, B, C, D>()
            where A : struct
            where B : struct
            where C : struct
            where D : struct
        {
            return QueryEntities(typeof(A), typeof(B), typeof(C), typeof(D));
        }

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
            return _entityArchetypes[entityID];
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
