using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Warlock : GateCard, IGateCard
    {
        public Warlock(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 4;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Open()
        {
            base.Open();

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" }, { "SourceType", "A" },
                { "SourceType", "G" },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_GATE_ABILITYNEGATETARGET", game.ActiveZone.Where(x=>x.ActiveType == Abilities.ActiveType.Effect).ToArray())
                } }
            });

            game.awaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            game.ActiveZone.First(x => x.EffectId == (int)game.IncomingSelection[Owner.Id]["array"][0]["active"]).Negate(false);

            game.ContinueGame();
        }

        public new void Remove()
        {
            base.Remove();
        }
    }
}
