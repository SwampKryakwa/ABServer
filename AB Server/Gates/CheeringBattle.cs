using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class CheeringBattle : GateCard
    {
        public CheeringBattle(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public override int TypeId { get; } = 3;

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
                        { "SelectionType", "BH" },
                        { "Message", "INFO_GATE_TARGET" },
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

            game.AwaitingAnswers[Owner.Id] = Setup;
        }

        Bakugan target;

        public void Setup()
        {
            target = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.ResolveChain();
        }

        public override void Resolve()
        {
            if (!counterNegated && target.InHands)
            {
                target.AddFromHand(this);
                var newPower = int.Parse(target.Power.ToString().Substring(1));
                target.Boost(new Boost((short)(newPower - target.Power)), this);
            }

            game.ContinueGame();
        }
    }
}
