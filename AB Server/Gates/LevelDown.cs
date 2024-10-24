using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class LevelDown : GateCard, IGateCard
    {
        public LevelDown(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 9;

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
                        { "SelectionType", "BF" },
                        { "Message", "INFO_GATE_TARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Bakugans.Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            game.awaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            Bakugan target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            if (target.Power > 400)
                target.Boost(new Boost(400 - target.Power), this);
            else
                target.Boost(new Boost(-100), this);

            game.ContinueGame();
        }

        public new void Remove()
        {
            base.Remove();
        }
    }
}
