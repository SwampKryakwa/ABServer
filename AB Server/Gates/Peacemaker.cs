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

        public override int TypeId { get; } = 1;

        public override void DetermineWinnerNormalBattle()
        {
            game.BattlesToEnd.Add(this);
        }

        public override void DetermineWinnerFakeBattle()
        {
            FakeBattleDraw();
        }
    }
}
