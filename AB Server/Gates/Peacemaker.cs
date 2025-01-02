using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class Peacemaker : GateCard
    {
        public Peacemaker(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 9;

        public override void Resolve()
        {

        }

        public override void DetermineWinner()
        {
            if (IsOpen)
            {
                game.BattlesToEnd.Add(this);
            }
            else
                base.DetermineWinner();
        }
    }
}
