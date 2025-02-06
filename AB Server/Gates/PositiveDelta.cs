using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class PositiveDelta : GateCard
    {
        public PositiveDelta(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 6;

        public override bool CheckBattles()
        {
            Console.WriteLine("Checking battle at " + Position + "...");
            if (IsFrozen || BattleOver) return false;

            Console.WriteLine("Number of Bakugan: " + Bakugans.Count);
            bool isBattle = Bakugans.Count > 1;

            if (isBattle)
            {
                Console.WriteLine("Must have a battle");

                if (!ActiveBattle)
                {
                    Console.WriteLine("Battle yet to be started... Adding battle to the initialization list");
                    game.BattlesToStart.Add(this);
                    Open();
                }
            }
            else
            {
                ActiveBattle = false;
            }

            return isBattle;
        }

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));
            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (Bakugans.Any(x=>x.Attribute == Attribute.Nova || x.Attribute == Attribute.Aqua || x.Attribute == Attribute.Lumina))
                foreach (var bakugan in Bakugans.Where(x => x.Attribute == Attribute.Nova || x.Attribute == Attribute.Aqua || x.Attribute == Attribute.Lumina))
                    bakugan.Boost(new Boost(-200), this);
            else
                foreach (var bakugan in Bakugans)
                    bakugan.Boost(new Boost(-200), this);
        }

        public override bool IsOpenable() =>
            false;
    }
}
