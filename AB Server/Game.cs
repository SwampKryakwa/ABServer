using AB_Server.Abilities;

using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AB_Server
{
    enum ActivationWindow : byte
    {
        Normal,
        BattleStart,
        BattleEnd,
        TurnEnd,
    }
    internal class Game
    {
        public List<JObject>[] NewEvents { get; set; }
        public JObject[] IncomingSelection;
        public Dictionary<long, int> UidToPid = [];

        public ushort PlayerCount;
        ushort loggedPlayers = 0;
        ushort playersPassed = 0;

        Dictionary<long, ushort> UUIDToPid = [];

        public List<Player> Players;
        public GateCard[,] Field;
        public List<IActive> ActiveZone = [];
        public int NextEffectId = 0;

        public GateCard? GetGateByCoord(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Field.GetLength(0) || Y >= Field.GetLength(1)) return null;
            return Field[X, Y];
        }

        public ushort[] Sides;
        public List<Bakugan> BakuganIndex = [];
        public List<GateCard> GateIndex = [];
        public List<AbilityCard> AbilityIndex = [];

        public ushort TurnPlayer;
        public ushort ActivePlayer { get; protected set; }
        public bool isBattleGoing = false;
        public ActivationWindow CurrentWindow = ActivationWindow.Normal;

        public List<AbilityCard> AbilityChain { get; set; } = [];

        //All the event types in the game
        public delegate void BakuganBoostedEffect(Bakugan target, Boost boost, object source);
        public delegate void BakuganPowerResetEffect(Bakugan bakugan);
        public delegate void BakuganMovedEffect(Bakugan target, BakuganContainer pos);
        public delegate void BakuganReturnedEffect(Bakugan target, ushort owner);
        public delegate void BakuganDestroyedEffect(Bakugan target, ushort owner);
        public delegate void BakuganRevivedEffect(Bakugan target, ushort owner);
        public delegate void BakuganThrownEffect(Bakugan target, ushort owner, BakuganContainer pos);
        public delegate void BakuganAddedEffect(Bakugan target, ushort owner, BakuganContainer pos);
        public delegate void BakuganPlacedFromGraveEffect(Bakugan target, ushort owner, BakuganContainer pos);
        public delegate void GateAddedEffect(GateCard target, ushort owner, params int[] pos);
        public delegate void GateRemovedEffect(GateCard target, ushort owner, params int[] pos);
        public delegate void BattleOverEffect(GateCard target);
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

        public void OnBakuganBoosted(Bakugan target, Boost boost, object source) =>
            BakuganBoosted?.Invoke(target, boost, source);
        public void OnBakuganMoved(Bakugan target, BakuganContainer pos) =>
            BakuganMoved?.Invoke(target, pos);
        public void OnBakuganAdded(Bakugan target, ushort owner, BakuganContainer pos) =>
            BakuganAdded?.Invoke(target, owner, pos);
        public void OnBakuganThrown(Bakugan target, ushort owner, BakuganContainer pos)
        {
            BakuganAdded?.Invoke(target, owner, pos);
            BakuganThrown?.Invoke(target, owner, pos);
        }
        public void OnBakuganPlacedFromGrave(Bakugan target, ushort owner, BakuganContainer pos) =>
            BakuganPlacedFromGrave?.Invoke(target, owner, pos);
        public void OnBakuganReturned(Bakugan target, ushort owner) =>
            BakuganReturned?.Invoke(target, owner);
        public void OnBakuganDestroyed(Bakugan target, ushort owner) =>
            BakuganDestroyed?.Invoke(target, owner);
        public void OnBakuganRevived(Bakugan target, ushort owner) =>
            BakuganRevived?.Invoke(target, owner);
        public void OnGateAdded(GateCard target, ushort owner, params int[] pos) =>
            GateAdded?.Invoke(target, owner, pos);
        public void OnGateRemoved(GateCard target, ushort owner, params int[] pos) =>
            GateRemoved?.Invoke(target, owner, pos);
        public void OnBattleOver(GateCard target) =>
            BattleOver?.Invoke(target);

        public Action[] AwaitingAnswers;

        public bool Started = false;
        bool Over = false;
        public int Left = 0;

        public bool DontThrowTurnStartEvent = false;

        public Game(ushort playerCount)
        {
            Field = new GateCard[2, 3];
            PlayerCount = playerCount;
            NewEvents = new List<JObject>[playerCount];
            AwaitingAnswers = new Action[playerCount];
            Players = new();
            IncomingSelection = new JObject[playerCount];
            for (ushort i = 0; i < playerCount; i++)
            {
                NewEvents[i] = new List<JObject>();
            }
        }

        public int AddPlayer(JObject deck, long UUID, string playerName)
        {
            Players.Add(Player.FromJson(loggedPlayers, loggedPlayers, deck, this, playerName));
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
            List<JObject> toReturn;
            try
            {
                toReturn = new(NewEvents[player]);
            }
            catch
            {
                Console.WriteLine(NewEvents.Length);
                Console.WriteLine(player);
                throw;
            }
            NewEvents[player].Clear();

            return toReturn;
        }

        public void Initiate()
        {
            try
            {
                Sides = Players.Select(x => x.SideID).ToArray();

                TurnPlayer = (ushort)new Random().Next(Players.Count);
                ActivePlayer = TurnPlayer;

                for (int i = 0; i < Players.Count; i++)
                {
                    var p = Players[i];
                    JArray gates = new();

                    for (int j = 0; j < p.GateHand.Count; j++)
                    {
                        int type = p.GateHand[j].TypeId;
                        switch (type)
                        {
                            //case 0:
                            //    Console.WriteLine(0);
                            //    gates.Add(new JObject { { "Type", type }, { "Attribute", (int)((NormalGate)p.GateHand[j]).Attribute }, { "Power", ((NormalGate)p.GateHand[j]).Power } });
                            //    break;
                            //case 4:
                            //    Console.WriteLine(4);
                            //    gates.Add(new JObject { { "Type", type }, { "Attribute", (int)((AttributeHazard)p.GateHand[j]).Attribute } });
                            //    break;
                            default:
                                gates.Add(new JObject { { "Type", type } });
                                break;
                        }
                    }
                    if (NewEvents[i].Count == 0)
                        NewEvents[i].Add(new JObject { { "Type", "PickGateEvent" }, { "Prompt", "pick_gate_start" }, { "Gates", gates } });
                }

                for (int i = 0; i < PlayerCount; i++)
                    AwaitingAnswers[i] = () =>
                    {
                        if (IncomingSelection.Contains(null)) return;
                        for (int i = 0; i < IncomingSelection.Length; i++)
                        {
                            JObject selection = IncomingSelection[i];
                            IncomingSelection[i] = null;

                            Players[i].GateHand[(int)selection["gate"]].Set(i, 1);

                            selection = null;
                        }

                        foreach (List<JObject> e in NewEvents) e.Add(new JObject { { "Type", "PlayerNamesInfo" }, { "Info", new JArray(Players.Select(x => x.DisplayName).ToArray()) } });
                        foreach (List<JObject> e in NewEvents) e.Add(new JObject { { "Type", "PlayerTurnStart" }, { "PID", ActivePlayer } });
                        Started = true;
                    };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public bool doNotMakeStep = false;

        public void GameStep()
        {
            JObject selection = IncomingSelection[ActivePlayer];

            string moveType = selection["Type"].ToString();

            DontThrowTurnStartEvent = false;
            if (moveType != "pass") playersPassed = 0;
            switch (moveType)
            {
                case "throw":
                    GateCard gateSelection = Field[(int)selection["posX"], (int)selection["posY"]];

                    if (gateSelection == null)
                    {
                        Console.WriteLine($"Gate {selection["posX"]} {selection["posY"]} is null");
                        NewEvents[TurnPlayer].Add(new JObject
                        {
                            ["Type"] = "InvalidAction"
                        });
                        break;
                    }

                    Players[TurnPlayer].HadThrownBakugan = true;
                    BakuganIndex[(int)selection["bakugan"]].Throw(gateSelection);

                    break;
                case "set":
                    (int X, int Y) posSelection = ((int)selection["posX"], (int)selection["posY"]);

                    if (Field[posSelection.X, posSelection.Y] != null)
                    {
                        NewEvents[TurnPlayer].Add(new JObject
                        {
                            ["Type"] = "InvalidAction"
                        });
                    }
                    else
                    {
                        Players[TurnPlayer].HadSetGate = true;
                        GateIndex[(int)selection["gate"]].Set(posSelection.X, posSelection.Y);
                    }

                    break;
                case "activate":
                    int abilitySelection = (int)selection["ability"];

                    if (!AbilityIndex[abilitySelection].IsActivateable())
                    {
                        NewEvents[ActivePlayer].Add(new JObject
                        {
                            ["Type"] = "InvalidAction"
                        });
                    }
                    else
                    {
                        DontThrowTurnStartEvent = true;
                        doNotMakeStep = true;
                        AbilityChain.Add(AbilityIndex[abilitySelection]);
                        AbilityIndex[abilitySelection].EffectId = NextEffectId++;
                        ActiveZone.Add(AbilityIndex[abilitySelection]);
                        Players[TurnPlayer].AbilityHand.Remove(AbilityIndex[abilitySelection]);

                        for (int i = 0; i < NewEvents.Length; i++)
                        {
                            NewEvents[i].Add(new()
                            {
                                ["Type"] = "AbilityAddedActiveZone",
                                ["IsCopy"] = AbilityIndex[abilitySelection].IsCopy,
                                ["Id"] = AbilityIndex[abilitySelection].EffectId,
                                ["Card"] = AbilityIndex[abilitySelection].TypeId,
                                ["Owner"] = AbilityIndex[abilitySelection].Owner.Id
                            });
                        }

                        AbilityIndex[abilitySelection].Setup(false);
                    }
                    break;
                case "open":
                    GateCard gateToOpen = GateIndex[(int)selection["gate"]];

                    if (gateToOpen == null)
                    {
                        NewEvents[ActivePlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }

                    if (gateToOpen.IsOpenable())
                    {
                        DontThrowTurnStartEvent = true;
                        doNotMakeStep = true;
                        gateToOpen.Open();
                    }
                    else
                        NewEvents[ActivePlayer].Add(new JObject { { "Type", "InvalidAction" } });

                    break;
                case "pass":
                    if (!isBattleGoing)
                    {
                        NewEvents[ActivePlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }
                    playersPassed++;
                    if (playersPassed == PlayerCount)
                    {
                        foreach (var g in Field.Cast<GateCard?>())
                            if (g?.ActiveBattle == true)
                                g.DetermineWinner();

                        int looser = -1;
                        foreach (var p in Players)
                            if (!p.BakuganOwned.Any(x => !x.Defeated))
                            {
                                looser = p.Id;
                                break;
                            }

                        if (looser != -1)
                        {
                            for (int i = 0; i < PlayerCount; i++)
                                NewEvents[i].Add(new JObject { { "Type", "GameOver" }, { "Victor", Players.First(x => x.Id != looser).Id } });
                            Over = true;
                            break;
                        }

                        isBattleGoing = false;

                        EndTurn();
                    }

                    break;
                case "end":
                    if (Players[TurnPlayer].HadSkippedTurn && Players[TurnPlayer].Bakugans.Count > 0 && !Players[TurnPlayer].HadThrownBakugan)
                    {
                        NewEvents[TurnPlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }

                    if (Players[TurnPlayer].CanEndTurn())
                    {
                        EndTurn();
                    }
                    else
                        NewEvents[TurnPlayer].Add(new JObject { { "Type", "InvalidAction" } });

                    break;
            }
            if (isBattleGoing)
            {
                ActivePlayer = (ushort)((ActivePlayer + 1) % PlayerCount);
            }
            if (Over) return;
            if (!DontThrowTurnStartEvent)
                ContinueGame();
        }

        public void EndTurn()
        {
            Console.WriteLine("Invoking turn end effects");
            TurnEnd?.Invoke();

            Console.WriteLine("Trying to end turn");
            if (isBattleGoing)
            {
                Console.WriteLine("Restarting battle");
                ActivePlayer = TurnPlayer;
                foreach (var e in NewEvents) e.Add(new JObject
                        {
                            { "Type", "PlayerTurnStart" },
                            { "PID", ActivePlayer }
                        });
            }
            else
            {
                if (Players[TurnPlayer].Bakugans.Count > 0 && !Players[TurnPlayer].HadThrownBakugan)
                    Players[TurnPlayer].HadSkippedTurn = true;

                Console.WriteLine("Ending turn");
                TurnPlayer = (ushort)((TurnPlayer + 1) % PlayerCount);
                ActivePlayer = TurnPlayer;

                BakuganIndex.ForEach(x => x.UsedAbilityThisTurn = false);

                if (!BakuganIndex.Any(x => x.InHand()))
                    foreach (var bakugan in BakuganIndex.Where(x => x.OnField()))
                        bakugan.ToHand((bakugan.Position as GateCard).EnterOrder);

                Players[TurnPlayer].HadSetGate = false;
                Players[TurnPlayer].HadThrownBakugan = false;
                Players[TurnPlayer].HadUsedFusion = false;
                Players[TurnPlayer].HadUsedCounter = false;
            }
        }

        public JObject GetPossibleMoves(int player)
        {
            JArray gateArray = new JArray();

            foreach (var gate in Players[player].SettableGates())
                switch (gate.TypeId)
                {
                    //case 0:
                    //    Console.WriteLine(0);
                    //    gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId }, { "Attribute", (int)((NormalGate)gate).Attribute }, { "Power", ((NormalGate)gate).Power } });
                    //    break;
                    //case 4:
                    //    Console.WriteLine(4);
                    //    gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId }, { "Attribute", (int)((AttributeHazard)gate).Attribute } });
                    //    break;
                    default:
                        gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId } });
                        break;
                }

            JArray bakuganArray = new JArray();

            foreach (var bakugan in Players[player].ThrowableBakugan())
                bakuganArray.Add(new JObject { { "BID", bakugan.BID }, { "Type", (int)bakugan.Type }, { "Attribute", (int)bakugan.Attribute }, { "Treatment", (int)bakugan.Treatment }, { "Power", bakugan.Power } });

            JObject moves = new()
            {
                { "CanSetGate", Players[player].HasSettableGates() && !isBattleGoing },
                { "CanOpenGate", Players[player].HasOpenableGates() },
                { "CanThrowBakugan", Players[player].HasThrowableBakugan() && GateIndex.Any(x=>x.OnField) },
                { "CanActivateAbility", Players[player].HasActivateableAbilities() && (Players[player].AbilityBlockers.Count == 0) },
                { "CanEndTurn", Players[player].CanEndTurn() },
                { "CanEndBattle", Players[player].CanEndBattle() },

                { "IsASkip", !Players[player].HadThrownBakugan },
                { "IsAPass", isBattleGoing && playersPassed != (PlayerCount - 1) },

                { "SettableGates", gateArray },
                { "OpenableGates", new JArray(Players[player].OpenableGates().Select(x => new JObject { { "CID", x.CardId } })) },
                { "ThrowableBakugan", bakuganArray },
                { "ActivateableAbilities", new JArray(Players[player].AbilityHand.Select(x => new JObject { { "cid", x.CardId }, { "Type", x.TypeId }, { "CanActivate", x.IsActivateable() } })) }
            };
            return moves;
        }

        public void CheckChain(Player player, AbilityCard ability, Bakugan user)
        {
            if (!player.HadUsedFusion && player.HasActivateableFusionAbilities(user))
                SuggestFusion(player, ability, user);
            else if (Players.Any(x => !x.HadUsedCounter && x.HasActivateableAbilities()))
            {
                int next = player.Id + 1;
                if (next == PlayerCount) next = 0;
                int initial = next;
                while (Players[next].HadUsedCounter || !Players[next].HasActivateableAbilities())
                {
                    next++;
                    if (next == PlayerCount) next = 0;
                    if (initial == next) break;
                }
                if (next == player.Id)
                {
                    ResolveChain();
                    return;
                }
                SuggestCounter(Players[next], ability, player);
            }
            else ResolveChain();
        }

        public void SuggestWindow(ActivationWindow window, int startingPlayer, int player)
        {
            CurrentWindow = window;
            var currentPlayer = Players[player];

            if (currentPlayer.HasActivateableAbilities())
            {
                AwaitingAnswers[player] = () => CheckWindow(startingPlayer, player);
                NewEvents[player].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent(window.ToString().ToUpper() + "WINDOWPROMPT")));
            }
            else
            {
                if (++player >= PlayerCount) player = 0;

                if (player == startingPlayer) ContinueGame();
                else SuggestWindow(window, startingPlayer, player);
            }
        }

        public void CheckWindow(int startingPlayer, int player)
        {
            if ((bool)IncomingSelection[player]["array"][0]["answer"])
            {

            }
            else
            {
                if (++player >= PlayerCount) player = 0;

                if (player == startingPlayer) ContinueGame();
                else SuggestWindow(CurrentWindow, startingPlayer, player);
            }
        }

        public void ResolveWindow(Player player)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateable())
            {
                NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection(CurrentWindow.ToString().ToUpper() + "WINDOWSELECTION", player.AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
            }
        }

        public void SuggestCounter(Player player, AbilityCard ability, Player user)
        {
            AwaitingAnswers[player.Id] = () => CheckCounter(player, ability, user);
            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.
                CounterSelectionEvent(user.Id, ability.TypeId)));
        }

        public void CheckCounter(Player player, AbilityCard ability, Player user)
        {
            if (!(bool)IncomingSelection[player.Id]["array"][0]["answer"])
            {
                int next = player.Id + 1;
                if (next == PlayerCount) next = 0;
                if (next == user.Id) ResolveChain();
                else SuggestCounter(Players[next], ability, user);
            }
            else
            {
                player.HadUsedCounter = true;
                AwaitingAnswers[player.Id] = () => ResolveCounter(player);

                NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection("CounterSelection", player.AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
            }
        }

        public void ResolveCounter(Player player)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateableCounter())
            {
                AbilityChain.Add(AbilityIndex[id]);
                AbilityIndex[id].EffectId = NextEffectId++;
                ActiveZone.Add(AbilityIndex[id]);
                player.AbilityHand.Remove(AbilityIndex[id]);

                for (int i = 0; i < NewEvents.Length; i++)
                {
                    NewEvents[i].Add(new()
                    {
                        ["Type"] = "AbilityAddedActiveZone",
                        ["IsCopy"] = AbilityIndex[id].IsCopy,
                        ["Id"] = AbilityIndex[id].EffectId,
                        ["Card"] = AbilityIndex[id].TypeId,
                        ["Owner"] = AbilityIndex[id].Owner.Id
                    });
                }

                AbilityIndex[id].Setup(true);
            }
        }

        public void SuggestFusion(Player player, AbilityCard ability, Bakugan user)
        {
            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("FUSIONPROMPT")));

            AwaitingAnswers[player.Id] = () => CheckFusion(player, ability, user);
        }

        public void CheckFusion(Player player, AbilityCard ability, Bakugan user)
        {
            if (!(bool)IncomingSelection[player.Id]["array"][0]["answer"])
            {
                if (Players.Any(x => !x.HadUsedCounter))
                {
                    int next = player.Id + 1;
                    if (next == PlayerCount) next = 0;
                    int initial = next;
                    while (Players[next].HadUsedCounter || !Players[next].HasActivateableAbilities())
                    {
                        next++;
                        if (next == PlayerCount) next = 0;
                        if (initial == next) break;
                    }
                    if (next == player.Id)
                    {
                        ResolveChain();
                        return;
                    }
                    SuggestCounter(Players[next], ability, player);
                }
                else ResolveChain();
                return;
            }

            player.HadUsedFusion = true;
            AwaitingAnswers[player.Id] = () => ResolveFusion(player, ability, user);

            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection("FusionSelection", player.AbilityHand.Where(x => x.IsActivateableFusion(user)).ToArray())));
        }

        public void ResolveFusion(Player player, AbilityCard ability, Bakugan user)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateableFusion(user))
            {
                AbilityChain.Insert(AbilityChain.IndexOf(ability) + 1, AbilityIndex[id]);
                AbilityIndex[id].EffectId = NextEffectId++;
                ActiveZone.Add(AbilityIndex[id]);
                player.AbilityHand.Remove(AbilityIndex[id]);

                for (int i = 0; i < NewEvents.Length; i++)
                {
                    NewEvents[i].Add(new()
                    {
                        ["Type"] = "AbilityAddedActiveZone",
                        ["IsCopy"] = AbilityIndex[id].IsCopy,
                        ["Id"] = AbilityIndex[id].EffectId,
                        ["Card"] = AbilityIndex[id].TypeId,
                        ["Owner"] = AbilityIndex[id].Owner.Id
                    });
                }

                AbilityIndex[id].SetupFusion(ability, user);
            }
        }

        public void ResolveChain()
        {
            Console.WriteLine("Resolving chain");
            AbilityChain.Reverse();
            foreach (AbilityCard ability in AbilityChain)
                ability.Resolve();
            AbilityChain.Clear();
            ContinueGame();
        }

        public void ContinueGame()
        {
            Console.WriteLine("Game step over");
            doNotMakeStep = false;
            foreach (var playerEvents in NewEvents)
                playerEvents.Add(new JObject { { "Type", "PlayerTurnStart" }, { "PID", ActivePlayer } });
        }
    }
}
