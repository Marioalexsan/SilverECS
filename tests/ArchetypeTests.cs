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
            long idIndex = 0;

            Archetype archetype = new Archetype(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC));

            int matching = 8;

            for (int i = 0; i < matching; i++)
            {
                Entity entity = new Entity(null, idIndex++);

                object[] components = new object[]
                {
                    new ComponentA(),
                    new ComponentB(),
                    new ComponentC(),
                };

                archetype.InjectEntity(entity, ComponentSet.From(components));

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
                Entity entity = archetype.GetEntities().First();

                archetype.ExtractEntity(entity, out var components);

                Assert.AreEqual(3, components.Types.Count());
                Assert.AreEqual(matching - i - 1, archetype.Count);

                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentA>().Count());
                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentB>().Count());
                Assert.AreEqual(matching - i - 1, archetype.GetComponents<ComponentC>().Count());
            }

            for (int i = 0; i < matching; i++)
            {
                Entity entity = new Entity(null, idIndex++);

                object[] components = new object[]
                {
                    new ComponentA(),
                    new ComponentB(),
                    new ComponentC(),
                };

                archetype.InjectEntity(entity, ComponentSet.From(components));

                Assert.IsTrue(archetype.GetComponent<ComponentA>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentB>(entity, out _));
                Assert.IsTrue(archetype.GetComponent<ComponentC>(entity, out _));
            }
        }

        [TestMethod]
        public void ArchetypeInfo()
        {
            Archetype archetypeOne = new Archetype(typeof(ComponentA), typeof(ComponentC));

            Assert.IsTrue(archetypeOne.HasType(typeof(ComponentA)));
            Assert.IsTrue(archetypeOne.HasType(typeof(ComponentC)));
            Assert.IsFalse(archetypeOne.HasType(typeof(ComponentD)));

            Archetype archetypeThree = new Archetype(typeof(ComponentA), typeof(ComponentB), typeof(ComponentC));

            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentA)));
            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentB)));
            Assert.IsTrue(archetypeThree.HasType(typeof(ComponentC)));
            Assert.IsFalse(archetypeThree.HasType(typeof(ComponentD)));
        }

        [TestMethod]
        public void Queries()
        {
            long idIndex = 0;

            Archetype archetype = new Archetype(typeof(ComponentA), typeof(ComponentC), typeof(ComponentD), typeof(ComponentE));

            int count = 5;

            for (int i = 0; i < count; i++)
            {
                Entity loopEntity = new Entity(null, idIndex++);

                object[] loopComponents = new object[]
                {
                    new ComponentA(),
                    new ComponentC(),
                    new ComponentD(),
                    new ComponentE(),
                };

                archetype.InjectEntity(loopEntity, ComponentSet.From(loopComponents));
            }

            Assert.AreEqual(count, archetype.GetComponents<ComponentA>().Count());
            Assert.AreEqual(null, archetype.GetComponents<ComponentB>());
            Assert.AreEqual(count, archetype.GetComponents<ComponentC>().Count());

            Entity entity = new Entity(null, 100);

            object[] components = new object[]
            {
                    new ComponentA(),
                    new ComponentC(),
                    new ComponentD(),
                    new ComponentE(),
            };

            archetype.InjectEntity(entity, ComponentSet.From(components));

            Assert.IsTrue(archetype.GetComponent<ComponentA>(entity, out _));
            Assert.IsFalse(archetype.GetComponent<ComponentB>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentC>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentD>(entity, out _));
            Assert.IsTrue(archetype.GetComponent<ComponentE>(entity, out _));

            Assert.AreEqual(count + 1, archetype.GetEntities().Count());
        }
    }
}
