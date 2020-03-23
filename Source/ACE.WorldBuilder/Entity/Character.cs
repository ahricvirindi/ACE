using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE.WorldBuilder.Entity
{
    public class Character
    {
        public uint AccountId { get; set; }
        public uint Id { get; set; }
        public int? Level { get; set; }
        public string Name { get; set;  }
    }
}
