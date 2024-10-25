using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class Transform : GateCard, IGateCard
    {
        public Transform(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 10;

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
                        { "SelectionBakugans", new JArray(Bakugans.Where(x => x.Owner == Owner).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            game.awaitingAnswers[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.NewEvents[Owner.Id].Add(new JObject {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
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

            game.awaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            target.Boost(new Boost(game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]].Power - target.Power), this);

            game.ContinueGame();
        }
    }
}
