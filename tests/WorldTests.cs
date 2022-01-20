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

            Entity entityA = world.CreateEntity();
            Entity entityB = world.CreateEntity();
            Entity entityC = world.CreateEntity();

            Assert.AreEqual(3, world.EntityCount);

            Entity entityD = world.CreateEntity();
            Entity entityE = world.CreateEntity();

            Assert.AreEqual(5, world.EntityCount);

            world.DestroyEntity(entityA);
            world.DestroyEntity(entityB);
            world.DestroyEntity(entityC);

            Assert.AreEqual(2, world.EntityCount);

            world.DestroyEntity(entityD);
            world.DestroyEntity(entityE);

            Assert.AreEqual(0, world.EntityCount);

            Entity entity = world.CreateEntity();

            // Entity component addition / deletion defers to the assigned world

            Assert.IsFalse(world.GetComponent<ComponentB>(entity, out _));

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentC>(entity);

            Assert.IsTrue(world.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(world.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(world.GetComponent<ComponentC>(entity, out _));

            world.RemoveComponent<ComponentA>(entity);

            Assert.IsFalse(world.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(world.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(world.GetComponent<ComponentC>(entity, out _));

            world.DestroyEntity(entity);

            Assert.AreEqual(0, world.EntityCount);
        }

        [TestMethod]
        public void Queries()
        {
            World world = new World();

            Entity entity;

            entity = world.CreateEntity();

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentC>(entity);

            entity = world.CreateEntity();

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentD>(entity);

            entity = world.CreateEntity();

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentB>(entity);
            world.AddComponent<ComponentC>(entity);
            world.AddComponent<ComponentD>(entity);



            entity = world.CreateEntity();

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentB>(entity);
            world.AddComponent<ComponentD>(entity);

            entity = world.CreateEntity();

            world.AddComponent<ComponentB>(entity);
            world.AddComponent<ComponentC>(entity);

            Assert.AreEqual(4, world.QueryEntities<ComponentA>().Count());
            Assert.AreEqual(3, world.QueryEntities<ComponentB>().Count());
            Assert.AreEqual(3, world.QueryEntities<ComponentC>().Count());
            Assert.AreEqual(3, world.QueryEntities<ComponentD>().Count());
            Assert.AreEqual(0, world.QueryEntities<ComponentE>().Count());

            Assert.AreEqual(2, world.QueryEntities<ComponentA, ComponentB>().Count());
            Assert.AreEqual(3, world.QueryEntities<ComponentA, ComponentD>().Count());
            Assert.AreEqual(0, world.QueryEntities<ComponentA, ComponentE>().Count());
            Assert.AreEqual(2, world.QueryEntities<ComponentB, ComponentC>().Count());

            Assert.AreEqual(1, world.QueryEntities<ComponentB, ComponentC, ComponentD>().Count());
            Assert.AreEqual(0, world.QueryEntities<ComponentB, ComponentC, ComponentE>().Count());

            Assert.AreEqual(1, world.QueryEntities<ComponentA, ComponentB, ComponentC, ComponentD>().Count());

            Assert.AreEqual(0, world.QueryEntities(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE)).Count());

            entity = world.CreateEntity();

            world.AddComponent<ComponentA>(entity);
            world.AddComponent<ComponentB>(entity);
            world.AddComponent<ComponentC>(entity);
            world.AddComponent<ComponentD>(entity);
            world.AddComponent<ComponentE>(entity);

            Assert.AreEqual(1, world.QueryEntities(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE)).Count());
        }
    }
}
