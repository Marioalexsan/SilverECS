using System;
using System.Collections.Generic;
using System.Collections;

namespace SilverECS
{
    /// <summary>
    /// Allows for simple data transfer between systems in a FIFO manner.
    /// </summary>
    public class MessageContainer : IMessageContainer
    {
        private Dictionary<Type, IList> _storage = new Dictionary<Type, IList>();

        /// <summary>
        /// Adds a message of the given type to the container.
        /// </summary>
        public void PushMessage<T>(T message)
            where T : struct
        {
            List<T> list = GetStorage<T>();

            list.Add(message);
        }

        /// <summary>
        /// Adds all of the messages of the given type to the container.
        /// </summary>
        public void PushMessages<T>(IEnumerable<T> messages)
            where T : struct
        {
            List<T> list = GetStorage<T>();

            list.AddRange(messages);
        }

        /// <summary>
        /// Removes a message of the given type from the container and returns it in the out parameter.
        /// Returns true if a message was retrieved, false if no messages were found.
        /// </summary>
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

        /// <summary>
        /// Removes all messages of the given type from the container (if any), and returns them in a list.
        /// </summary>
        public List<T> PopMessages<T>()
            where T : struct
        {
            List<T> list = GetStorage<T>();

            _storage[typeof(T)] = new List<T>(list.Capacity / 2 + 1);

            return list;
        }

        internal void Clear()
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