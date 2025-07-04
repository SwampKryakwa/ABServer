using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class Shockwave : GateCard
    {
        public Shockwave(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 15;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Push(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!Negated)
            {
                foreach (var gate in game.GateIndex.Where(x => x.OnField))
                    gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
                foreach (var gate in game.GateIndex.Where(x => x.OnField && x.IsAdjacent(this)))
                    gate.Bakugans.ForEach(x => x.Boost(new Boost(-100), this));
            }
            game.ChainStep();
        }
    }
}
