using AB_Server.Abilities;
using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Warlock : GateCard
    {
        public Warlock(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 2;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(EventBuilder.GateOpen(this));

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_GATE_ABILITYNEGATETARGET", game.ActiveZone.Where(x => x is not GateCard && x is not AbilityCard).ToArray())
                } }
            });

            game.AwaitingAnswers[Owner.Id] = Setup;
        }

        IActive target;

        public void Setup()
        {
            target = game.ActiveZone.First(x => x.EffectId == (int)game.IncomingSelection[Owner.Id]["array"][0]["active"]);

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!counterNegated)
                target.Negate();
        }

        public override bool IsOpenable() => game.ActiveZone.Count != 0 && base.IsOpenable();
    }
}
