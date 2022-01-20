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
    internal class Archetype
    {
        private const int PreallocationSize = 256;

        private Type[] filter;

        private Dictionary<Entity, int> entityIndex = new(PreallocationSize);

        private List<Entity> slotEntities = new List<Entity>(PreallocationSize);

        private Dictionary<Type, IList> componentLists;

        public int Count => entityIndex.Count;

        public IReadOnlyList<Type> Filter { get; private set; }

        public Archetype(params Type[] filter) : this(filter.AsEnumerable()) { }

        public Archetype(IEnumerable<Type> filter)
        {
            this.filter = filter.Distinct().ToArray();

            this.componentLists = new Dictionary<Type, IList>(this.filter.Length);

            foreach (Type type in filter)
            {
                Type listType = typeof(List<>).MakeGenericType(type);

                IList list = (IList)Activator.CreateInstance(listType, PreallocationSize);

                componentLists.Add(type, list);
            }

            this.Filter = Array.AsReadOnly(this.filter);
        }

        public bool HasType(Type type)
        {
            return filter.Any(x => x == type);
        }

        public IEnumerable<T> GetComponents<T>()
        {
            return GetComponentStorage<T>();
        }

        public bool GetComponent(Entity entityID, Type type, out object component)
        {
            if (filter.Length == 0 || !entityIndex.TryGetValue(entityID, out int index))
            {
                component = default;
                return false;
            }
            else
            {
                IList list = componentLists[type];

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

        public bool GetComponent<T>(Entity entityID, out T component)
        {
            if (filter.Length == 0 || !entityIndex.TryGetValue(entityID, out int index))
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

        public bool SetComponent(Entity entityID, Type type, object component)
        {
            if (filter.Length == 0 || !entityIndex.TryGetValue(entityID, out int index))
            {
                return false;
            }
            else
            {
                IList list = componentLists[type];

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

        public bool SetComponent<T>(Entity entityID, in T component)
        {
            if (filter.Length == 0 || !entityIndex.TryGetValue(entityID, out int index))
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

        public IEnumerable<Entity> GetEntities()
        {
            return entityIndex.Keys;
        }

        public bool HasEntity(Entity entityID)
        {
            return entityIndex.ContainsKey(entityID);
        }

        public bool AddEntity(Entity entityID)
        {
            if (HasEntity(entityID))
            {
                return false;
            }

            foreach (KeyValuePair<Type, IList> pair in componentLists)
            {
                pair.Value.Add(Activator.CreateInstance(pair.Key));
            }

            entityIndex.Add(entityID, Count);
            slotEntities.Add(entityID);
            return true;
        }

        public bool InjectEntity(Entity entityID, ComponentSet set)
        {
            if (!AddEntity(entityID))
            {
                return false;
            }

            for (int i = 0; i < set.Components.Count; i++)
            {
                SetComponent(entityID, set.Types[i], set.Components[i]);
            }

            return true;
        }

        public bool RemoveEntity(Entity entityID)
        {
            if (!HasEntity(entityID))
            {
                return false;
            }

            SwapSets(entityIndex[entityID], Count - 1);

            int index = Count - 1;

            foreach (KeyValuePair<Type, IList> pair in componentLists)
            {
                pair.Value.RemoveAt(index);
            }

            entityIndex.Remove(entityID);
            slotEntities.RemoveAt(index);
            return true;
        }

        public bool ExtractEntity(Entity entityID, out ComponentSet set)
        {
            if (!HasEntity(entityID))
            {
                set = default;
                return false;
            }

            set.Components = new List<object>();
            set.Types = new List<Type>();

            for (int i = 0; i < filter.Length; i++)
            {
                GetComponent(entityID, filter[i], out object component);

                set.Components.Add(component);
                set.Types.Add(filter[i]);
            }

            RemoveEntity(entityID);
            return true;
        }

        private List<T> GetComponentStorage<T>()
        {
            foreach (IList container in componentLists.Values)
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

            Entity entityA = slotEntities[indexA];
            Entity entityB = slotEntities[indexB];

            slotEntities[indexA] = entityB;
            slotEntities[indexB] = entityA;

            foreach (Type type in filter)
            {
                IList list = componentLists[type];

                object componentSwap = list[indexA];
                list[indexA] = list[indexB];
                list[indexB] = componentSwap;
            }

            int indexSwap = entityIndex[entityA];
            entityIndex[entityA] = entityIndex[entityB];
            entityIndex[entityB] = indexSwap;
        }

        /// <summary>
        /// Returns a debug view of the archetype.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');

            foreach (Type type in filter)
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
