using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    [DebuggerDisplay("Filter = {ToString()}, Entities = {Count}")]
    internal class EntityArchetype
    {
        private const int PreallocationSize = 256;

        private Type[] _filter;

        private Dictionary<EntityID, int> _entityToIndex = new Dictionary<EntityID, int>(PreallocationSize);

        private List<EntityID> _locationToEntity = new List<EntityID>(PreallocationSize);

        private Dictionary<Type, IList> _componentLists;

        public int Count => _entityToIndex.Count;

        public IReadOnlyList<Type> Filter { get; private set; }

        public EntityArchetype(params Type[] filter) : this(filter.AsEnumerable()) { }

        public EntityArchetype(IEnumerable<Type> filter)
        {
            if (filter.Any(x => !x.IsValueType || x.IsPrimitive))
            {
                throw new InvalidOperationException("Components must be non-primitive value types.");
            }

            this._filter = filter.Distinct().ToArray();

            this._componentLists = new Dictionary<Type, IList>(this._filter.Length);

            foreach (Type type in filter)
            {
                Type listType = typeof(List<>).MakeGenericType(type);

                IList list = (IList)Activator.CreateInstance(listType, PreallocationSize);

                _componentLists.Add(type, list);
            }

            Filter = Array.AsReadOnly(_filter);
        }

        public bool HasType(Type type)
        {
            return _filter.Any(x => x == type);
        }

        public IEnumerable<T> GetComponents<T>()
            where T : struct
        {
            return GetComponentStorage<T>();
        }

        public bool GetComponent(EntityID entityID, Type type, out object component)
        {
            if (_filter.Length == 0 || !_entityToIndex.TryGetValue(entityID, out int index))
            {
                component = default;
                return false;
            }
            else
            {
                IList list = _componentLists[type];

                if (list != null)
                {
                    component = list[index];
                    return true;
                }
                else
                {
                    component = default;
                    return false;
                }
            }
        }

        public bool GetComponent<T>(EntityID entityID, out T component)
            where T : struct
        {
            if (_filter.Length == 0 || !_entityToIndex.TryGetValue(entityID, out int index))
            {
                component = default;
                return false;
            }
            else
            {
                List<T> list = GetComponentStorage<T>();

                if (list != null)
                {
                    component = list[index];
                    return true;
                }
                else
                {
                    component = default;
                    return false;
                }
            }
        }

        public bool SetComponent(EntityID entityID, Type type, object component)
        {
            if (_filter.Length == 0 || !_entityToIndex.TryGetValue(entityID, out int index))
            {
                return false;
            }
            else
            {
                IList list = _componentLists[type];

                if (list != null)
                {
                    list[index] = component;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SetComponent<T>(EntityID entityID, in T component)
            where T : struct
        {
            if (_filter.Length == 0 || !_entityToIndex.TryGetValue(entityID, out int index))
            {
                return false;
            }
            else
            {
                List<T> list = GetComponentStorage<T>();

                if (list != null)
                {
                    list[index] = component;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerable<EntityID> GetEntities()
        {
            return _entityToIndex.Keys;
        }

        public bool HasEntity(EntityID entityID)
        {
            return _entityToIndex.ContainsKey(entityID);
        }

        public bool AddEntity(EntityID entityID)
        {
            if (HasEntity(entityID))
            {
                return false;
            }

            foreach (KeyValuePair<Type, IList> pair in _componentLists)
            {
                pair.Value.Add(Activator.CreateInstance(pair.Key));
            }

            _entityToIndex.Add(entityID, Count);
            _locationToEntity.Add(entityID);
            return true;
        }

        public bool AddAndPopulateEntity(EntityID entityID, IEnumerable<object> components)
        {
            if (!AddEntity(entityID))
            {
                return false;
            }

            foreach (object component in components)
            {
                SetComponent(entityID, component.GetType(), component);
            }

            return true;
        }

        public bool RemoveEntity(EntityID entityID)
        {
            if (!HasEntity(entityID))
            {
                return false;
            }

            SwapSets(_entityToIndex[entityID], Count - 1);

            int index = Count - 1;

            foreach (KeyValuePair<Type, IList> pair in _componentLists)
            {
                pair.Value.RemoveAt(index);
            }

            _entityToIndex.Remove(entityID);
            _locationToEntity.RemoveAt(index);
            return true;
        }

        public bool ExtractAndRemoveEntity(EntityID entityID, out List<object> components)
        {
            if (!HasEntity(entityID))
            {
                components = null;
                return false;
            }

            components = new List<object>();

            for (int i = 0; i < _filter.Length; i++)
            {
                GetComponent(entityID, _filter[i], out object component);
                components.Add(component);
            }

            RemoveEntity(entityID);
            return true;
        }

        private List<T> GetComponentStorage<T>()
            where T : struct
        {
            foreach (IList container in _componentLists.Values)
            {
                if (container is List<T> list)
                {
                    return list;
                }
            }

            return null;
        }

        private void SwapSets(int indexA, int indexB)
        {
            if (indexA == indexB)
            {
                return;
            }

            EntityID entityA = _locationToEntity[indexA];
            EntityID entityB = _locationToEntity[indexB];

            _locationToEntity[indexA] = entityB;
            _locationToEntity[indexB] = entityA;

            foreach (Type type in _filter)
            {
                IList list = _componentLists[type];

                object componentSwap = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = componentSwap;
            }

            int indexSwap = _entityToIndex[entityA];
            _entityToIndex[entityA] = _entityToIndex[entityB];
            _entityToIndex[entityB] = indexSwap;
        }

        /// <summary>
        /// Returns a debug view of the archetype.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');

            foreach (Type type in _filter)
            {
                builder.Append(type.Name);
                builder.Append(", ");
            }

            if (builder.Length > 2)
            {
                builder.Length -= 2;
            }
            else
            {
                builder.Append("<Empty>");
            }

            builder.Append(']');

            return builder.ToString();
        }
    }
}
