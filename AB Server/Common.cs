using AB_Server.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server
{
    internal interface IActive
    {
        public int EffectId { get; set; }
        public int TypeId { get; }
        public AbilityKind Kind { get; }
        public Bakugan User { get; set; }

        public Player Owner { get; set; }

        public void Negate(bool asCounter = false);
    }

    internal interface IChainable
    {
        public void Resolve();
    }

    interface IBakuganContainer
    {
        List<Bakugan> Bakugans { get; }

        public void Remove(Bakugan bakugan)
        {
            Bakugans.Remove(bakugan);
        }

        public void Add(Bakugan bakugan)
        {
            Bakugans.Add(bakugan);
        }
    }
}
