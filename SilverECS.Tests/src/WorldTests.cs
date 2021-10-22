using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SilverECS.Tests
{
    [TestClass]
    public class WorldTests
    {
        [TestMethod]
        public void EntityManipulation()
        {
            World world = new World();

            EntityID entityA = world.ECManager.CreateEntity();
            EntityID entityB = world.ECManager.CreateEntity();
            EntityID entityC = world.ECManager.CreateEntity();

            Assert.AreEqual(3, world.ECManager.EntityCount);

            EntityID entityD = world.ECManager.CreateEntity();
            EntityID entityE = world.ECManager.CreateEntity();

            Assert.AreEqual(5, world.ECManager.EntityCount);

            world.ECManager.DestroyEntity(entityA);
            world.ECManager.DestroyEntity(entityB);
            world.ECManager.DestroyEntity(entityC);

            Assert.AreEqual(2, world.ECManager.EntityCount);

            world.ECManager.DestroyEntity(entityD);
            world.ECManager.DestroyEntity(entityE);

            Assert.AreEqual(0, world.ECManager.EntityCount);

            EntityID entity = world.ECManager.CreateEntity();

            // Entity component addition / deletion defers to the assigned world

            Assert.IsFalse(world.ECManager.GetComponent<ComponentB>(entity, out _));

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentC>(entity);

            Assert.IsTrue(world.ECManager.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(world.ECManager.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(world.ECManager.GetComponent<ComponentC>(entity, out _));

            world.ECManager.RemoveComponent<ComponentA>(entity);

            Assert.IsFalse(world.ECManager.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(world.ECManager.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(world.ECManager.GetComponent<ComponentC>(entity, out _));

            world.ECManager.DestroyEntity(entity);

            Assert.AreEqual(0, world.ECManager.EntityCount);
        }

        [TestMethod]
        public void Queries()
        {
            World world = new World();

            EntityID entity;

            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentC>(entity);

            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentD>(entity);

            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentB>(entity);
            world.ECManager.AddComponent<ComponentC>(entity);
            world.ECManager.AddComponent<ComponentD>(entity);



            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentB>(entity);
            world.ECManager.AddComponent<ComponentD>(entity);

            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentB>(entity);
            world.ECManager.AddComponent<ComponentC>(entity);

            Assert.AreEqual(4, world.ECManager.QueryEntities<ComponentA>().Count());
            Assert.AreEqual(3, world.ECManager.QueryEntities<ComponentB>().Count());
            Assert.AreEqual(3, world.ECManager.QueryEntities<ComponentC>().Count());
            Assert.AreEqual(3, world.ECManager.QueryEntities<ComponentD>().Count());
            Assert.AreEqual(0, world.ECManager.QueryEntities<ComponentE>().Count());

            Assert.AreEqual(2, world.ECManager.QueryEntities<ComponentA, ComponentB>().Count());
            Assert.AreEqual(3, world.ECManager.QueryEntities<ComponentA, ComponentD>().Count());
            Assert.AreEqual(0, world.ECManager.QueryEntities<ComponentA, ComponentE>().Count());
            Assert.AreEqual(2, world.ECManager.QueryEntities<ComponentB, ComponentC>().Count());

            Assert.AreEqual(1, world.ECManager.QueryEntities<ComponentB, ComponentC, ComponentD>().Count());
            Assert.AreEqual(0, world.ECManager.QueryEntities<ComponentB, ComponentC, ComponentE>().Count());

            Assert.AreEqual(1, world.ECManager.QueryEntities<ComponentA, ComponentB, ComponentC, ComponentD>().Count());

            Assert.AreEqual(0, world.ECManager.QueryEntities(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE)).Count());

            entity = world.ECManager.CreateEntity();

            world.ECManager.AddComponent<ComponentA>(entity);
            world.ECManager.AddComponent<ComponentB>(entity);
            world.ECManager.AddComponent<ComponentC>(entity);
            world.ECManager.AddComponent<ComponentD>(entity);
            world.ECManager.AddComponent<ComponentE>(entity);

            Assert.AreEqual(1, world.ECManager.QueryEntities(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE)).Count());
        }
    }
}
