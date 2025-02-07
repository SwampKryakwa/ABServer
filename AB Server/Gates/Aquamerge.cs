using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class Aquamerge : GateCard
    {
        public Aquamerge(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 7;

        public override void Open()
        {
            foreach (var bakugan in Bakugans.Where(x => x.Attribute != Attribute.Aqua))
                bakugan.ChangeAttribute(Attribute.Aqua, this);
        }
    }
}
