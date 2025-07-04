using AB_Server.Abilities;
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
                { "Count", 1 },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_GATE_ABILITYNEGATETARGET", TypeId, (int)Kind, game.ActiveZone.Where(x => x is not GateCard && x is not AbilityCard).ToArray())
                } }
            });

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
