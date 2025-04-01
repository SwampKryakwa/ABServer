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
            if (IsOpen)
            {
                foreach (Bakugan b in Bakugans)
                {
                    b.BattleEndedInDraw = true;
                }
                game.BattlesToEnd.Add(this);
            }
            else
                base.DetermineWinnerNormalBattle();
        }

        public override void DetermineWinnerFakeBattle()
        {
            if (IsOpen)
                FakeBattleDraw();
            else
                base.DetermineWinnerFakeBattle();
        }
    }
}
