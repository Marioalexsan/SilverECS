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
        private Dictionary<Type, IList> storage = new Dictionary<Type, IList>();

        /// <summary>
        /// Adds a message of the given type to the container.
        /// </summary>
        public void PushMessage<Message>(Message message)
        {
            List<Message> list = GetStorage<Message>();

            list.Add(message);
        }

        /// <summary>
        /// Adds all of the messages of the given type to the container.
        /// </summary>
        public void PushMessages<Message>(IEnumerable<Message> messages)
        {
            List<Message> list = GetStorage<Message>();

            list.AddRange(messages);
        }

        /// <summary>
        /// Removes a message of the given type from the container and returns it in the out parameter.
        /// Returns true if a message was retrieved, false if no messages were found.
        /// </summary>
        public bool PopMessage<Message>(out Message message)
        {
            List<Message> list = GetStorage<Message>();

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
        public IEnumerable<Message> PopMessages<Message>()
        {
            List<Message> list = GetStorage<Message>();

            storage[typeof(Message)] = new List<Message>(list.Capacity / 2 + 1);

            return list;
        }

        internal void Clear()
        {
            foreach (IList list in storage.Values)
            {
                list.Clear();
            }
        }

        private List<Message> GetStorage<Message>()
        {
            if (storage.TryGetValue(typeof(Message), out IList container))
            {
                return (List<Message>)container;
            }
            else
            {
                List<Message> list = new List<Message>();

                storage[typeof(Message)] = list;

                return list;
            }
        }
    }
}