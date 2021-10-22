using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverECS.Tests
{
    public struct ComponentA
    {
        public int ValueA;

        public bool ValueB;
    }

    public struct ComponentB
    {
        public string ValueA;

        public float ValueB;

        public List<string> ValueC;
    }

    public struct ComponentC
    {

    }

    public struct ComponentD
    {
        public string ValueA;
    }

    public struct ComponentE
    {
        public string ValueA;

        public string ValueB;

        public int ValueC;

        public string ValueD;
    }
}
