using AB_Server.Abilities;

using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace AB_Server
{
    internal class Game
    {
        public List<JObject>[] NewEvents;
        public JObject[] IncomingSelection;
        public Dictionary<long, int> UidToPid = new();

        public ushort PlayerCount;
        ushort loggedPlayers = 0;
        ushort playersPassed = 0;

        Dictionary<long, ushort> UUIDToPid = new();

        public List<Player> Players;
        public GateCard[,] Field;

        public ushort[] Sides;
        public List<Bakugan> BakuganIndex = new();
        public List<IGateCard> GateIndex = new();
        public List<IAbilityCard> AbilityIndex = new();

        ushort turnPlayer;
        public ushort activePlayer { get; protected set; }
        public bool isFightGoing = false;

        public List<INegatable> NegatableAbilities = new();

        //All the event types in the game
        public delegate void BakuganBoostedEffect(Bakugan target, short boost);
        public delegate void BakuganPowerResetEffect(Bakugan bakugan);
        public delegate void BakuganMovedEffect(Bakugan target, int pos);
        public delegate void BakuganReturnedEffect(Bakugan target, ushort owner);
        public delegate void BakuganDestroyedEffect(Bakugan target, ushort owner);
        public delegate void BakuganRevivedEffect(Bakugan target, ushort owner);
        public delegate void BakuganThrownEffect(Bakugan target, ushort owner, int pos);
        public delegate void BakuganAddedEffect(Bakugan target, ushort owner, int pos);
        public delegate void BakuganPlacedFromGraveEffect(Bakugan target, ushort owner, int pos);
        public delegate void GateAddedEffect(IGateCard target, ushort owner, int pos);
        public delegate void GateRemovedEffect(IGateCard target, ushort owner, int pos);
        public delegate void BattleOverEffect(IGateCard target, ushort winner);
        public delegate void TurnEndEffect();

        //All the events in the game
        public event BakuganBoostedEffect BakuganBoosted;
        public event BakuganPowerResetEffect BakuganPowerReset;
        public event BakuganMovedEffect BakuganMoved;
        public event BakuganReturnedEffect BakuganReturned;
        public event BakuganDestroyedEffect BakuganDestroyed;
        public event BakuganRevivedEffect BakuganRevived;
        public event BakuganThrownEffect BakuganThrown;
        public event BakuganAddedEffect BakuganAdded;
        public event BakuganPlacedFromGraveEffect BakuganPlacedFromGrave;
        public event GateAddedEffect GateAdded;
        public event GateRemovedEffect GateRemoved;
        public event BattleOverEffect BattleOver;
        public event TurnEndEffect TurnEnd;

        public void OnBakuganBoosted(Bakugan target, short boost) =>
            BakuganBoosted?.Invoke(target, boost);
        public void OnBakuganMoved(Bakugan target, int pos) =>
            BakuganMoved?.Invoke(target, pos);
        public void OnBakuganAdded(Bakugan target, ushort owner, int pos)
        {
            BakuganAdded?.Invoke(target, owner, pos);
            BakuganThrown?.Invoke(target, owner, pos);
        }
        public void OnBakuganThrown(Bakugan target, ushort owner, int pos) =>
            BakuganThrown?.Invoke(target, owner, pos);
        public void OnBakuganPlacedFromGrave(Bakugan target, ushort owner, int pos) =>
            BakuganPlacedFromGrave?.Invoke(target, owner, pos);
        public void OnBakuganReturned(Bakugan target, ushort owner) =>
            BakuganReturned?.Invoke(target, owner);
        public void OnBakuganDestroyed(Bakugan target, ushort owner) =>
            BakuganDestroyed?.Invoke(target, owner);
        public void OnBakuganRevived(Bakugan target, ushort owner) =>
            BakuganRevived?.Invoke(target, owner);
        public void OnGateAdded(IGateCard target, ushort owner, int pos) =>
            GateAdded?.Invoke(target, owner, pos);
        public void OnGateRemoved(IGateCard target, ushort owner, int pos) =>
            GateRemoved?.Invoke(target, owner, pos);
        public void OnBattleOver(IGateCard target, ushort winner) =>
            BattleOver?.Invoke(target, winner);

        public Action[] awaitingAnswers;

        public bool Started = false;
        bool Over = false;
        public int Left = 0;

        public Game(ushort playerCount)
        {
            Field = new GateCard[2, 3];
            PlayerCount = playerCount;
            NewEvents = new List<JObject>[playerCount];
            awaitingAnswers = new Action[playerCount];
            Players = new();
            IncomingSelection = new JObject[playerCount];
            for (ushort i = 0; i < playerCount; i++)
            {
                NewEvents[i] = new List<JObject>();
            }
        }

        public int AddPlayer(JObject deck, long UUID)
        {
            Players.Add(Player.FromJson(loggedPlayers, loggedPlayers, deck, this));
            UUIDToPid.Add(UUID, loggedPlayers);
            loggedPlayers++;
            return loggedPlayers - 1;
        }

        public ushort GetPid(long UUID)
        {
            return UUIDToPid[UUID];
        }

        public List<JObject> GetUpdates(int player)
        {
            int numberUpdates = NewEvents[player].Count;
            List<JObject> toReturn = new();
            if (NewEvents[player].Count != 0) toReturn = NewEvents[player].Slice(0, numberUpdates);
            NewEvents[player] = NewEvents[player].Skip(numberUpdates).ToList();

            return toReturn;
        }

        public void Initiate()
        {
            Sides = Players.Select(x => x.SideID).ToArray();

            turnPlayer = (ushort)new Random().Next(Players.Count);
            activePlayer = turnPlayer;

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                JArray gates = new();

                for (int j = 0; j < p.GateHand.Count; j++)
                {
                    int type = p.GateHand[j].GetTypeID();
                    if (type == 0)
                        gates.Add(new JObject { { "Type", type }, { "Attribute", (int)((NormalGate)p.GateHand[j]).Attribute }, { "Power", ((NormalGate)p.GateHand[j]).Power } });
                    else
                        gates.Add(new JObject { { "Type", type } });
                }
                NewEvents[i].Add(new JObject { { "Type", "PickGateEvent" }, { "Prompt", "pick_gate_start" }, { "Gates", gates } });
            }

            for (int i = 0; i < PlayerCount; i++)
                awaitingAnswers[i] = () =>
                {
                    if (IncomingSelection.Contains(null)) return;
                    for (int i = 0; i < IncomingSelection.Length; i++)
                    {
                        JObject selection = IncomingSelection[i];
                        IncomingSelection[i] = null;

                        Players[i].GateHand[(int)selection["gate"]].SetStart(i * 10 + 1);

                        selection = null;
                    }

                    foreach (var e in NewEvents) e.Add(new JObject { { "Type", "PlayerTurnStart" }, { "PID", activePlayer } });
                    Started = true;
                };
        }

        public void GameStep()
        {
            JObject selection = IncomingSelection[activePlayer];

            string moveType = selection["Type"].ToString();

            bool dontThrowTurnStartEvent = false;
            if (moveType != "pass") playersPassed = 0;
            switch (moveType)
            {
                case "throw":
                    int gateSelection = (int)selection["pos"];


                    if (Field[gateSelection / 10, gateSelection % 10] == null)
                    {
                        NewEvents[turnPlayer].Add(new JObject()
                        {
                            { "Type", "InvalidAction" }
                        });
                        break;
                    }


                    if (!Field[gateSelection / 10, gateSelection % 10].DisallowedPlayers[activePlayer])
                    {
                        Players[turnPlayer].HasThrownBakugan = true;
                        BakuganIndex[(int)selection["bakugan"]].Throw(gateSelection);
                    }
                    else
                        NewEvents[turnPlayer].Add(new JObject()
                        {
                            { "Type", "InvalidAction" }
                        });

                    break;
                case "set":
                    int posSelection = (int)selection["pos"];


                    if (Field[posSelection / 10, posSelection % 10] != null)
                    {
                        NewEvents[turnPlayer].Add(new JObject()
                        {
                            { "Type", "InvalidAction" }
                        });
                    }
                    else
                    {
                        Players[turnPlayer].HasSetGate = true;
                        GateIndex[(int)selection["gate"]].Set(posSelection);
                    }

                    break;
                case "activate":
                    int abilitySelection = (int)selection["ability"];

                    if (!AbilityIndex[abilitySelection].IsActivateable())
                    {
                        NewEvents[activePlayer].Add(new JObject()
                        {
                            { "Type", "InvalidAction" }
                        });
                    }
                    else
                    {
                        dontThrowTurnStartEvent = true;
                        AbilityIndex[abilitySelection].Activate();
                    }
                    break;
                case "open":
                    IGateCard gateToOpen = GateIndex[(int)selection["gate"]];

                    if (gateToOpen == null)
                    {
                        NewEvents[activePlayer].Add(new JObject() { { "Type", "InvalidAction" } });
                        break;
                    }

                    if (gateToOpen.IsOpenable())
                        gateToOpen.Open();
                    else
                        NewEvents[activePlayer].Add(new JObject() { { "Type", "InvalidAction" } });

                    break;
                case "pass":
                    if (!isFightGoing)
                    {
                        NewEvents[activePlayer].Add(new JObject() { { "Type", "InvalidAction" } });
                        break;
                    }
                    playersPassed++;
                    if (playersPassed == PlayerCount)
                    {
                        foreach (var g in Field.Cast<GateCard?>())
                        {
                            if (g?.ActiveBattle == true)
                            {
                                g.DetermineWinner();
                            }
                        }

                        int looser = -1;
                        foreach (var p in Players)
                        {
                            if (!p.BakuganOwned.Any(x => !x.Defeated))
                            {
                                looser = p.ID;
                                break;
                            }
                        }

                        if (looser != -1)
                        {
                            for (int i = 0; i < PlayerCount; i++)
                                NewEvents[i].Add(new JObject { { "Type", "GameOver" }, { "Victor", Players.First(x => x.ID != looser).ID } });
                            Over = true;
                            break;
                        }

                        isFightGoing = false;

                        TurnEnd?.Invoke();
                        if (!Players[turnPlayer].HasThrownBakugan) Players[turnPlayer].HasSkippedTurn = true;

                        turnPlayer = (ushort)((turnPlayer + 1) % PlayerCount);
                        activePlayer = turnPlayer;

                        BakuganIndex.ForEach(x => x.usedAbilityThisTurn = false);

                        Players[turnPlayer].HasSetGate = false;
                        Players[turnPlayer].HasThrownBakugan = false;
                    }

                    break;
                case "end":
                    if (Players[turnPlayer].CanEndTurn())
                    {
                        TurnEnd?.Invoke();
                        if (!Players[turnPlayer].HasThrownBakugan) Players[turnPlayer].HasSkippedTurn = true;

                        turnPlayer = (ushort)((turnPlayer + 1) % PlayerCount);
                        activePlayer = turnPlayer;

                        BakuganIndex.ForEach(x => x.usedAbilityThisTurn = false);

                        Players[turnPlayer].HasSetGate = false;
                        Players[turnPlayer].HasThrownBakugan = false;
                    }
                    else
                        NewEvents[turnPlayer].Add(new JObject { { "Type", "InvalidAction" } });

                    break;
            }
            if (isFightGoing)
            {
                activePlayer = (ushort)((activePlayer + 1) % PlayerCount);
            }
            if (Over) return;
            if (dontThrowTurnStartEvent)
                for (int i = 0; i < NewEvents.Length; i++)
                {
                    if (i != activePlayer)
                        NewEvents[i].Add(new JObject
                        {
                            { "Type", "PlayerTurnStart" },
                            { "PID", activePlayer }
                        });
                }
            else
                foreach (var e in NewEvents) e.Add(new JObject
                        {
                            { "Type", "PlayerTurnStart" },
                            { "PID", activePlayer }
                        });
        }


        public JObject GetPossibleMoves(int player)
        {
            JArray gateArray = new JArray();

            foreach (var gate in Players[player].SettableGates())
                if (gate.GetTypeID() != 0)
                    gateArray.Add(new JObject { { "CID", gate.CID }, { "Type", gate.GetTypeID() } });
                else
                    gateArray.Add(new JObject { { "CID", gate.CID }, { "Type", gate.GetTypeID() }, { "Attribute", (int)((NormalGate)gate).Attribute }, { "Power", ((NormalGate)gate).Power } });

            JArray bakuganArray = new JArray();

            foreach (var bakugan in Players[player].ThrowableBakugan())
                bakuganArray.Add(new JObject { { "BID", bakugan.BID }, { "Type", (int)bakugan.Type }, { "Attribute", (int)bakugan.Attribute }, { "Treatment", (int)bakugan.Treatment }, { "Power", bakugan.Power } });

            JObject moves = new()
            {
                { "CanSetGate", Players[player].HasSettableGates() && !isFightGoing },
                { "CanOpenGate", Players[player].HasOpenableGates() },
                { "CanThrowBakugan", Players[player].HasThrowableBakugan() },
                { "CanActivateAbility", Players[player].HasActivatableAbilities() },
                { "CanEndTurn", Players[player].CanEndTurn() },
                { "CanEndBattle", Players[player].CanEndBattle() },

                { "IsASkip", !Players[player].HasThrownBakugan },
                { "IsAPass", playersPassed != (PlayerCount - 1) },

                { "SettableGates", gateArray },
                { "OpenableGates", new JArray(Players[player].OpenableGates().Select(x => new JObject { { "CID", x.CID } })) },
                { "ThrowableBakugan", bakuganArray },
                { "ActivateableAbilities", new JArray(Players[player].ActivateableAbilities().Select(x => new JObject { { "cid", x.CID }, { "Type", x.GetTypeID() } })) }
            };
            return moves;
        }

        public void AskCounter(int player, IAbilityCard ability)
        {
            if (Players[player].HasActivatableAbilities(true))
                NewEvents[player].Add(new JObject
                {
                    { "Type", "StartSelection" },
                    { "SelectionType", "Q" },
                    { "Message", "want_counter" }
                });

            awaitingAnswers[player] = new Action(() => ResolveCounter(player, ability));
        }

        public void ResolveCounter(int player, IAbilityCard ability)
        {
            if (!(bool)IncomingSelection[player]["answer"])
            {
                if (ability.Iter()) ability.Resolve();
                return;
            }

            NewEvents[player].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "C" },
                { "Message", "choose_counter" },
                { "ActivateableAbilities", new JArray(Players[player].ActivateableAbilities().Select(x => new JObject { { "cid", x.CID }, { "Type", x.GetTypeID() } })) }
            });

            awaitingAnswers[player] = new Action(() => ResolveCounter(player, ability));
        }

        public void TriggerCounter(int player, IAbilityCard ability)
        {
            AbilityIndex[(int)IncomingSelection[player]["CID"]].ActivateCounter();
        }
    }
}
