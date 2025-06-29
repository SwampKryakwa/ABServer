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
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            game.ThrowEvent(EventBuilder.GateOpen(this));

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_GATE_ABILITYNEGATETARGET", TypeId, (int)Kind, game.ActiveZone.Where(x => x is not GateCard && x is not AbilityCard).ToArray())
                } }
            });

            game.OnAnswer[Owner.Id] = Activate;
        }

        public void Activate()
        {
            IActive target = game.AbilityIndex.First(x => x.EffectId == (int)game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]);

            if (!Negated)
                target.Negate();

            game.ChainStep();
        }

        public override bool IsOpenable() => game.ActiveZone.Any(x => x is not GateCard && x is not AbilityCard) && base.IsOpenable();
    }
}
