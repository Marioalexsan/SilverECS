using System.Collections.Generic;

namespace SilverECS
{
    public interface IMessageContainer
    {
        bool PopMessage<Message>(out Message message);

        IEnumerable<Message> PopMessages<Message>();

        void PushMessage<Message>(Message message);

        void PushMessages<Message>(IEnumerable<Message> messages);
    }
}