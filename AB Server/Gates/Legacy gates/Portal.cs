using Newtonsoft.Json.Linq;
using System.Numerics;

namespace AB_Server.Gates
{
    internal class Portal : GateCard, IGateCard
    {
        public Portal(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;

            CardId = cID;
        }

        public new int TypeId { get; private protected set; } = 7;

        List<Bakugan> targets;

        public new void Open()
        {
            base.Open();

            targets = new List<Bakugan>();
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

            game.AwaitingAnswers[Owner.Id] = SuggestMore;
        }

        public void SuggestMore()
        {
            targets.Add(game.BakuganIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]]);

            game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("INFO_SELECTMORE")));

            game.AwaitingAnswers[Owner.Id] = CheckSelectMore;
        }

        public void CheckSelectMore()
        {
            if ((bool)game.IncomingSelection[Owner.Id]["array"][0]["answer"])
                SelectMore();
            else
                SelectDestination();
        }

        public void SelectMore()
        {
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

            game.AwaitingAnswers[Owner.Id] = SuggestMore;
        }

        public void SelectDestination()
        {
            game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(game.GateIndex.Where(x => x != this && !x.IsOpen && x.Owner == Owner && x.GetType() == typeof(Portal)).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            game.AwaitingAnswers[Owner.Id] = Resolve;
        }

        public void Resolve()
        {
            var target = game.GateIndex[(int)game.IncomingSelection[Owner.Id]["array"][0]["gate"]] as GateCard;


            for (int i = 0; i < game.PlayerCount; i++)
                game.NewEvents[i].Add(new()
                    {
                        { "Type", "GateOpenEvent" },
                        { "PosX", target.Position.X },
                        { "PosY", target.Position.Y },
                        { "GateData", new JObject {
                            { "Type", (target as IGateCard).TypeId } }
                        },
                        { "Owner", target.Owner.Id },
                        { "CID", target.CardId }
                    });

            Bakugan.MultiMove(game, target, MoveSource.Effect, targets.ToArray());

            game.ContinueGame();
        }

        public new bool IsOpenable() =>
            base.IsOpenable() && game.GateIndex.Any(x => x != this && !x.IsOpen && x.Owner == Owner && x.GetType() == typeof(Portal));
    }
}