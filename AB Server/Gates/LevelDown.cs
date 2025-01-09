using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class LevelDown : GateCard
    {
        public LevelDown(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 0;

        public override void Open()
        {
            IsOpen = true;
            game.ActiveZone.Add(this);
            game.CardChain.Add(this);
            EffectId = game.NextEffectId++;
            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(new()
                    {
                        { "Type", "GateOpenEvent" },
                        { "PosX", Position.X },
                        { "PosY", Position.Y },
                        { "GateData", new JObject {
                            { "Type", TypeId } }
                        },
                        { "Owner", Owner.Id },
                        { "CID", CardId }
                    });

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

            game.AwaitingAnswers[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!counterNegated && target.Power >= 400)
                target.Boost(new Boost(-100), this);
        }
    }
}
