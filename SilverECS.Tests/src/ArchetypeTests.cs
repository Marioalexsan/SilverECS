using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SilverECS.Tests
{
    [TestClass]
    public class ArchetypeTests
    {
        [TestMethod]
        public void EntityManipulation()
        {
            EntityArchetype archetype = new EntityArchetype(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC));

            int matching = 8;
            int nonmatching = 9;

            for (int i = 0; i < matching; i++)
            {
                EntityID entity = new EntityID(i);

                object[] components = new object[]
                {
                    new ComponentA(),
                    new ComponentB(),
                    new ComponentC(),
                };

                archetype.AddAndPopulateEntity(entity, components);

                Assert.IsTrue(archetype.GetComponent<ComponentA>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentB>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentC>(entity, out _));
            }

            Assert.AreEqual(matching, archetype.Count);

            Assert.AreEqual(matching, archetype.GetComponents<ComponentA>().Count());
            Assert.AreEqual(matching, archetype.GetComponents<ComponentB>().Count());
            Assert.AreEqual(matching, archetype.GetComponents<ComponentC>().Count());

            int todelete = 6;

            for (int i = 0; i < todelete; i++)
            {
                EntityID entity = archetype.GetEntities().First();

                archetype.ExtractAndRemoveEntity(entity, out var components);

                Assert.AreEqual(3, components.Count());
                Assert.AreEqual(matching - i - 1, archetype.Count);

                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentA>().Count());
                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentB>().Count());
                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentC>().Count());
            }

            for (int i = 0; i < matching; i++)
            {
                EntityID entity = new EntityID();

                object[] components = new object[]
                {
                    new ComponentA(),
                    new ComponentB(),
                    new ComponentC(),
                };

                archetype.AddAndPopulateEntity(entity, components);

                Assert.IsTrue(archetype.GetComponent<ComponentA>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentB>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentC>(entity, out _));
            }
        }

        [TestMethod]
        public void ArchetypeInfo()
        {
            EntityArchetype archetypeOne = new EntityArchetype(typeof(ComponentA), typeof(ComponentC));

            Assert.IsTrue(archetypeOne.HasType(typeof(ComponentA)));
            Assert.IsTrue(archetypeOne.HasType(typeof(ComponentC)));
            Assert.IsFalse(archetypeOne.HasType(typeof(ComponentD)));

            EntityArchetype archetypeThree = new EntityArchetype(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC));

            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentA)));
            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentB)));
            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentC)));
            Assert.IsFalse(archetypeThree.HasType(typeof(ComponentD)));
        }

        [TestMethod]
        public void Queries()
        {
            EntityArchetype archetype = new EntityArchetype(typeof(ComponentA), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE));

            int count = 5;

            for (int i = 0; i < count; i++)
            {
                EntityID loopEntity = new EntityID(i);

                object[] loopComponents = new object[]
                {
                    new ComponentA(),
                    new ComponentC(),
                    new ComponentD(),
                    new ComponentE(),
                };

                archetype.AddAndPopulateEntity(loopEntity, loopComponents);
            }

            Assert.AreEqual(count, archetype.GetComponents<ComponentA>().Count());
            Assert.AreEqual(null, archetype.GetComponents<ComponentB>());
            Assert.AreEqual(count, archetype.GetComponents<ComponentC>().Count());

            EntityID entity = new EntityID(100);

            object[] components = new object[]
            {
                    new ComponentA(),
                    new ComponentC(),
                    new ComponentD(),
                    new ComponentE(),
            };

            archetype.AddAndPopulateEntity(entity, components);

            Assert.IsTrue(archetype.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(archetype.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentC>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentD>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentE>(entity, out _));

            Assert.AreEqual(count + 1, archetype.GetEntities().Count());
        }
    }
}
