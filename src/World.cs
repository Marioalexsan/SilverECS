using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    [DebuggerDisplay("Update Systems = {UpdateStage.Count}, Renderer = {RenderStage}, Entities = {ECManager.EntityCount}")]
    public class World : IEntityManager, IMessageContainer
    {
        private Dictionary<int, Action<Entity>> _factories = new();

        private ArchetypeManager _archetypeManager;

        private MessageContainer _messageContainer;

        public Dictionary<string, List<ISystem>> Pipelines { get; } = new();

        public Dictionary<string, object> NamedObjects { get; } = new();

        public float TimeDilation { get; } = 1f;

        public int EntityCount => _archetypeManager.EntityCount;

        public World()
        {
            _archetypeManager = new ArchetypeManager(this);
            _messageContainer = new MessageContainer();
        }

        public void RunPipeline(string pipelineName, double deltaTime)
        {
            if (!Pipelines.TryGetValue(pipelineName, out var pipeline))
            {
                throw new InvalidOperationException("The pipeline with the given name doesn't exist!");
            }

            UpdateSettings updateSettings = new()
            {
                TimeDilation = TimeDilation
            };

            foreach (ISystem system in pipeline)
            {
                system.Update(this, deltaTime, updateSettings);
            }

            _messageContainer.Clear();
        }

        /// <summary>
        /// Sets a factory for an entity type.
        /// Afterwards, you can specify that entity type in <see cref="CreateEntity(int)"/>
        /// to create an entity using that factory.
        /// </summary>
        public void SetFactory(int entityType, Action<Entity> factory)
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
        /// Creates an entity using its entity type's factory, and returns its ID.
        /// </summary>
        public Entity CreateEntity(int entityType)
        {
            Entity entity = CreateEntity();

            if (_factories.ContainsKey(entityType))
            {
                _factories[entityType].Invoke(entity);
            }

            return entity;
        }

        #region Implemented interface methods

        public Entity CreateEntity()
        {
            return _archetypeManager.CreateEntity();
        }

        public bool DestroyEntity(Entity entityID)
        {
            return _archetypeManager.DestroyEntity(entityID);
        }

        public bool HasEntity(Entity entityID)
        {
            return _archetypeManager.HasEntity(entityID);
        }

        public Entity GetParent(Entity entityID)
        {
            return _archetypeManager.GetParent(entityID);
        }

        public bool AddComponent<Component>(Entity entityID, in Component component = default)
        {
            return _archetypeManager.AddComponent(entityID, component);
        }

        public bool GetComponent<Component>(Entity entityID, out Component component)
        {
            return _archetypeManager.GetComponent(entityID, out component);
        }

        public bool HasComponent<Component>(Entity entityID)
        {
            return _archetypeManager.HasComponent<Component>(entityID);
        }

        public bool RemoveComponent<Component>(Entity entityID)
        {
            return _archetypeManager.RemoveComponent<Component>(entityID);
        }

        public bool SetComponent<Component>(Entity entityID, in Component component)
        {
            return _archetypeManager.SetComponent(entityID, component);
        }

        public IEnumerable<Entity> QueryEntities<A>()
        {
            return _archetypeManager.QueryEntities<A>();
        }

        public IEnumerable<Entity> QueryEntities<A, B>()
        {
            return _archetypeManager.QueryEntities<A, B>();
        }

        public IEnumerable<Entity> QueryEntities<A, B, C>()
        {
            return _archetypeManager.QueryEntities<A, B, C>();
        }

        public IEnumerable<Entity> QueryEntities<A, B, C, D>()
        {
            return _archetypeManager.QueryEntities<A, B, C, D>();
        }

        public IEnumerable<Entity> QueryEntities(params Type[] types)
        {
            return _archetypeManager.QueryEntities(types);
        }

        public bool PopMessage<Message>(out Message message)
        {
            return _messageContainer.PopMessage(out message);
        }

        public IEnumerable<Message> PopMessages<Message>()
        {
            return _messageContainer.PopMessages<Message>();
        }

        public void PushMessage<Message>(Message message)
        {
            _messageContainer.PushMessage(message);
        }

        public void PushMessages<Message>(IEnumerable<Message> messages)
        {
            _messageContainer.PushMessages(messages);
        }

        #endregion
    }
}
