using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS
{
    public class World
    {
        public List<ISystem> UpdateStage { get; } = new List<ISystem>();

        public ISystem RenderStage { get; set; }

        public ECManager ECManager { get; } = new ECManager();

        public MessageBus MessageBus { get; } = new MessageBus();

        /// <summary>
        /// Runs all systems in UpdateStage.
        /// </summary>
        public void Update(double gameTime, double realTime)
        {
            foreach (ISystem system in UpdateStage)
            {
                system.Update(this, gameTime, realTime);
            }

            MessageBus.Clear();
        }

        /// <summary>
        /// Runs the RenderStage system.
        /// use this if your framework separates Update and Render (*cough* FNA - XNA *cough*).
        /// </summary>
        public void Render(double gameTime, double realTime)
        {
            RenderStage?.Update(this, gameTime, realTime);

            MessageBus.Clear();
        }
    }
}
