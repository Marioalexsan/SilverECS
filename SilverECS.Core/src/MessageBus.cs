using System;
using System.Collections.Generic;
using System.Collections;

namespace SilverECS
{
    public class MessageBus
    {
        private Dictionary<Type, IList> _storage = new Dictionary<Type, IList>();

        public void PushMessage<T>(T message)
            where T : struct
        {
            List<T> list = GetStorage<T>();

            list.Add(message);
        }

        public void PushMessages<T>(IEnumerable<T> messages)
            where T : struct
        {
            List<T> list = GetStorage<T>();

            list.AddRange(messages);
        }

        public bool PopMessage<T>(out T message)
            where T : struct
        {
            List<T> list = GetStorage<T>();

            if (list.Count > 0)
            {
                message = list[0];
                list.RemoveAt(0);

                return true;
            }
            else
            {
                message = default;
                return false;
            }
        }

        public List<T> PopMessages<T>()
            where T : struct
        {
            List<T> list = GetStorage<T>();

            _storage[typeof(T)] = new List<T>(list.Capacity / 2 + 1);

            return list;
        }

        public void Clear()
        {
            foreach (IList list in _storage.Values)
            {
                list.Clear();
            }
        }

        private List<T> GetStorage<T>()
            where T : struct
        {
            if (_storage.TryGetValue(typeof(T), out IList container))
            {
                return (List<T>)container;
            }
            else
            {
                List<T> list = new List<T>();

                _storage[typeof(T)] = list;

                return list;
            }
        }
    }
}