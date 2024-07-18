using Newtonsoft.Json.Linq;

namespace AB_Server.Gates
{
    internal class EnergyMerge : GateCard, IGateCard
    {
        public EnergyMerge(int cID, Player owner)
        {
            game = owner.game;
            Owner = owner;
            DisallowedPlayers = new bool[game.PlayerCount];
            for (int i = 0; i < game.PlayerCount; i++)
            {
                DisallowedPlayers[i] = false;
            }
            CardId = cID;
        }

        Bakugan first = null;
        Bakugan last = null;

        public new int TypeId { get; } = 5;

        public new void Negate()
        {
            IsOpen = false;
            Negated = true;
            if (first?.affectingEffects.Contains(this) == true)
            {
                first.Boost(-100, this);
            }
            if (last?.affectingEffects.Contains(this) == true)
            {
                last.Boost(-100, this);
            }
        }

        public new void Open()
        {
            if (EnterOrder.Count == 0)
            {
                IsOpen = true;
                return;
            }

            if (EnterOrder[0].Length == 1)
                first = EnterOrder[0][0];

            if (EnterOrder[^1].Length == 1)
            {
                last = EnterOrder[^1][0];
                if (first != null) TryResolve();
            }

            if (EnterOrder[0].Length != 1)
            {
                game.NewEvents[Owner.ID].Add(new JObject
                    {
                        { "Type", "StartSelectionGate" },
                        { "SelectionType", "B" },
                        { "Message", "gate_boost_target" },
                        { "gate", 5 },
                        { "SelectionBakugans", new JArray(EnterOrder[0].Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    });
                game.awaitingAnswers[Owner.ID] = TryResolve;
                return;
            }

            if (EnterOrder[^1].Length != 1)
            {
                game.NewEvents[Owner.ID].Add(new JObject
                    {
                        { "Type", "StartSelectionGate" },
                        { "SelectionType", "B" },
                        { "Message", "gate_deboost_target" },
                        { "gate", 5 },
                        { "SelectionBakugans", new JArray(EnterOrder[^1].Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    });
                game.awaitingAnswers[Owner.ID] = TryResolve;
                return;
            }
        }

        public void TryResolve()
        {
            if (first == null)
            {
                first = game.BakuganIndex[(int)game.IncomingSelection[Owner.ID]["bakugan"]];
                if (last == null)
                {
                    game.NewEvents[Owner.ID].Add(new JObject
                    {
                        { "Type", "StartSelectionGate" },
                        { "SelectionType", "B" },
                        { "Message", "gate_deboost_target" },
                        { "gate", 5 },
                        { "SelectionBakugans", new JArray(EnterOrder[^1].Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) }
                    });
                    game.awaitingAnswers[Owner.ID] = TryResolve;
                    return;
                }
            }

            else if (last == null)
                last = game.BakuganIndex[(int)game.IncomingSelection[Owner.ID]["bakugan"]];

            IsOpen = true;

            first.Boost(100, this);
            first.affectingEffects.Add(this);
            last.Boost(100, this);
            last.affectingEffects.Add(this);
        }

        public new void Remove()
        {
            IsOpen = false;
            TryUnfreeze(this);

            base.Remove();
        }
    }
}
