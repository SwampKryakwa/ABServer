﻿using AB_Server.Abilities;
using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Warlock : GateCard
    {
        public Warlock(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 2;

        public override void Resolve()
        {

            game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false,
                EventBuilder.ActiveSelection("INFO_GATE_ABILITYNEGATETARGET", TypeId, (int)Kind, game.ActiveZone.Where(x => x is not GateCard && x is not AbilityCard).ToArray())
            ));

            game.OnAnswer[Owner.Id] = Setup;
        }

        public void Setup()
        {
            IActive target = game.ActiveZone.First(x => x.EffectId == (int)game.PlayerAnswers[Owner.Id]!["array"][0]["active"]);

            if (!Negated)
                target.Negate();

            game.ChainStep();
        }

        public override bool IsOpenable() => game.ActiveZone.Any(x => x is not GateCard && x is not AbilityCard) && base.IsOpenable();
    }
}
