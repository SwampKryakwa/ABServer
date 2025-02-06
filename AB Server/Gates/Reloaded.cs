using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AB_Server.Gates
{
    internal class Reloaded : GateCard
    {
        public Reloaded(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            CardId = cID;
        }

        public override int TypeId { get; } = 10;

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

            game.AwaitingAnswers[Owner.Id] = Setup1;
        }

        Bakugan target1;
        Bakugan target2;

        public void Setup1()
        {
            target1 = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.NewEvents[Owner.Id].Add(new JObject {
                    { "Type", "StartSelection" },
                    { "Selections", new JArray {
                        new JObject {
                            { "SelectionType", "BF" },
                            { "Message", "INFO_GATE_TARGET" },
                            { "Ability", TypeId },
                            { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.Owner == Owner && x.Position != this && x.OnField()).Select(x =>
                                new JObject { { "Type", (int)x.Type },
                                    { "Attribute", (int)x.Attribute },
                                    { "Treatment", (int)x.Treatment },
                                    { "Power", x.Power },
                                    { "Owner", x.Owner.Id },
                                    { "BID", x.BID } })) }
                        }
                    } }
                });

            game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            target2 = game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            game.CheckChain(Owner, this);
        }

        public override void Resolve()
        {
            if (!counterNegated)
            {
                if (target1.OnField())
                {
                    target1.Boost(new Boost(100), this);
                }
                if (target2.OnField())
                {
                    target2.Boost(new Boost(-100), this);
                }
            }
        }
    }
}
