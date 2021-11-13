using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    [DebuggerDisplay("Update Systems = {UpdateStage.Count}, Renderer = {RenderStage}, Entities = {ECManager.EntityCount}")]
    public class World : IEntityComponentManager, IMessageContainer
    {
        public List<ISystem> UpdateStage { get; } = new List<ISystem>();

        public ISystem RenderStage { get; set; }

        private EntityComponentManager _entityComponentManager;

        private MessageContainer _messageContainer;

        public float TimeDilationFactor { get; } = 1f;

        public int EntityCount => _entityComponentManager.EntityCount;

        public World()
        {
            _entityComponentManager = new EntityComponentManager(this);
            _messageContainer = new MessageContainer();
        }

        /// <summary>
        /// Runs all systems in UpdateStage.
        /// </summary>
        public void Update(double deltaTime)
        {
            foreach (ISystem system in UpdateStage)
            {
                system.Update(this, deltaTime * TimeDilationFactor, deltaTime);
            }

            _messageContainer.Clear();
        }

        /// <summary>
        /// Runs the RenderStage system.
        /// Use this if your framework separates Update and Render stages (such as FNA).
        /// </summary>
        public void Render(double deltaTime)
        {
            RenderStage?.Update(this, deltaTime * TimeDilationFactor, deltaTime);

            _messageContainer.Clear();
        }

        #region Implemented interface methods

        public bool AddComponent<T>(EntityID entityID) where T : struct
        {
            return _entityComponentManager.AddComponent<T>(entityID);
        }

        public bool AddComponent<T>(EntityID entityID, in T component) where T : struct
        {
            return _entityComponentManager.AddComponent(entityID, component);
        }

        public EntityID CreateEntity()
        {
            return _entityComponentManager.CreateEntity();
        }

        public EntityID CreateEntity(int entityType)
        {
            return _entityComponentManager.CreateEntity(entityType);
        }

        public bool DestroyEntity(EntityID entityID)
        {
            return _entityComponentManager.DestroyEntity(entityID);
        }

        public bool EntityExists(EntityID entityID)
        {
            return _entityComponentManager.EntityExists(entityID);
        }

        public bool TryGetComponent<T>(EntityID entityID, out T component) where T : struct
        {
            return _entityComponentManager.TryGetComponent(entityID, out component);
        }

        public bool HasComponent<T>(EntityID entityID) where T : struct
        {
            return _entityComponentManager.HasComponent<T>(entityID);
        }

        public EntityID GetParent(EntityID entityID)
        {
            return _entityComponentManager.GetParent(entityID);
        }

        public bool HasFactory(int entityType)
        {
            return _entityComponentManager.HasFactory(entityType);
        }

        public IEnumerable<EntityID> QueryEntities<A>() where A : struct
        {
            return _entityComponentManager.QueryEntities<A>();
        }

        public IEnumerable<EntityID> QueryEntities<A, B>()
            where A : struct
            where B : struct
        {
            return _entityComponentManager.QueryEntities<A, B>();
        }

        public IEnumerable<EntityID> QueryEntities<A, B, C>()
            where A : struct
            where B : struct
            where C : struct
        {
            return _entityComponentManager.QueryEntities<A, B, C>();
        }

        public IEnumerable<EntityID> QueryEntities<A, B, C, D>()
            where A : struct
            where B : struct
            where C : struct
            where D : struct
        {
            return _entityComponentManager.QueryEntities<A, B, C, D>();
        }

        public IEnumerable<EntityID> QueryEntities(params Type[] types)
        {
            return _entityComponentManager.QueryEntities(types);
        }

        public bool RemoveComponent<T>(EntityID entityID) where T : struct
        {
            return _entityComponentManager.RemoveComponent<T>(entityID);
        }

        public bool SetComponent<T>(EntityID entityID, in T component) where T : struct
        {
            return _entityComponentManager.SetComponent(entityID, component);
        }

        public void SetFactory(int entityType, Action<EntityID, World> factory)
        {
            _entityComponentManager.SetFactory(entityType, factory);
        }

        public bool PopMessage<T>(out T message) where T : struct
        {
            return _messageContainer.PopMessage(out message);
        }

        public List<T> PopMessages<T>() where T : struct
        {
            return _messageContainer.PopMessages<T>();
        }

        public void PushMessage<T>(T message) where T : struct
        {
            _messageContainer.PushMessage(message);
        }

        public void PushMessages<T>(IEnumerable<T> messages) where T : struct
        {
            _messageContainer.PushMessages(messages);
        }

        #endregion
    }
}
