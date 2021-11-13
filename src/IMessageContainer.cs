using System.Collections.Generic;

namespace SilverECS
{
    public interface IMessageContainer
    {
        bool PopMessage<T>(out T message) where T : struct;
        List<T> PopMessages<T>() where T : struct;
        void PushMessage<T>(T message) where T : struct;
        void PushMessages<T>(IEnumerable<T> messages) where T : struct;
    }
}