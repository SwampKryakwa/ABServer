using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    internal class ResonanceCircuit : GateCard
    {
        public ResonanceCircuit(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 14;

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

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    EventBuilder.AbilitySelection("INFO_GATE_ABILITYTARGET", game.AbilityIndex.Where(x => x.Owner == Owner && x.Kind == CardKind.CorrelationAbility))
                } }
            });

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            AbilityCard target = game.AbilityIndex[(int)game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            if (!Negated)
                target.FromDropToHand();

            game.ChainStep();
        }

        public override bool IsOpenable() => game.ActiveZone.Any(x => x is not GateCard && x is not AbilityCard) && base.IsOpenable()
            && Owner.BakuganOwned.DistinctBy(x => x.Type).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BaseAttribute).Count() == Owner.BakuganOwned.Count
            && Owner.BakuganOwned.DistinctBy(x => x.BasePower).Count() == Owner.BakuganOwned.Count;
    }
}
