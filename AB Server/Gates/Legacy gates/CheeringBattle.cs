using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class CheeringBattle : GateCard, IGateCard
    {
        public CheeringBattle(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 2;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
        }

        public new void Open()
        {
            base.Open();

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BH" },
                        { "Message", "INFO_GATE_BOOSTTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Owner.Bakugans.Select(x =>
                        new JObject { { "Type", (int)x.Type },
                            { "Attribute", (int)x.Attribute },
                            { "Treatment", (int)x.Treatment },
                            { "Power", x.Power },
                            { "Owner", x.Owner.Id },
                            { "BID", x.BID } }
                        )) }
                    }
                } }
            });

            game.AwaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            var target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            target.AddFromHand(this);
            var newPower = int.Parse(target.Power.ToString().Substring(1));
            target.Boost(new Boost(newPower - target.Power), this);

            game.ContinueGame();
        }
    }
}
