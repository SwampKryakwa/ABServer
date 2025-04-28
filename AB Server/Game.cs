using AB_Server.Abilities;

using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Timers;

namespace AB_Server
{
    enum ActivationWindow : byte
    {
        Normal,
        BattleStart,
        BattleEnd,
        TurnStart,
        TurnEnd,
    }
    internal class Game
    {
        public List<JObject>[] NewEvents { get; set; }
        public Dictionary<long, List<JObject>> SpectatorEvents = [];
        public dynamic?[] IncomingSelection;
        public Dictionary<long, int> UidToPid = [];

        public byte PlayerCount;
        public byte SideCount;
        byte loggedPlayers = 0;
        byte playersPassedCount = 0;
        readonly List<Player> playersPassed = [];
        int currentTurn = 1;

        Dictionary<long, byte> UUIDToPid = [];

        public List<Player> Players;
        public List<long> Spectators = [];
        public GateCard?[,] Field;
        public List<IActive> ActiveZone = [];
        public int NextEffectId = 0;

        public GateCard? GetGateByCoord(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Field.GetLength(0) || Y >= Field.GetLength(1)) return null;
            return Field[X, Y];
        }

        //Indexes
        public List<Bakugan> BakuganIndex = [];
        public List<GateCard> GateIndex = [];
        public List<AbilityCard> AbilityIndex = [];

        public byte TurnPlayer { get; set; }
        public byte ActivePlayer { get; protected set; }
        public bool isBattleGoing { get => GateIndex.Any(x => x.OnField && x.ActiveBattle); }
        public ActivationWindow CurrentWindow = ActivationWindow.Normal;

        public List<IChainable> CardChain { get; set; } = [];
        public List<GateCard> BattlesToStart = [];
        public List<GateCard> AutoGatesToOpen = [];

        public List<GateCard> BattlesToEnd { get; } = [];
        public List<GateCard> GateSetList = [];

        //All the event types in the game
        public delegate void BakuganBoostedEffect(Bakugan target, Boost boost, object source);
        public delegate void BakuganPowerResetEffect(Bakugan bakugan);
        public delegate void BakuganMovedEffect(Bakugan target, IBakuganContainer pos);
        public delegate void BakuganReturnedEffect(Bakugan target, byte owner);
        public delegate void BakuganDestroyedEffect(Bakugan target, byte owner);
        public delegate void BakuganRevivedEffect(Bakugan target, byte owner);
        public delegate void BakuganThrownEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganAddedEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganPlacedFromGraveEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void GateAddedEffect(GateCard target, byte owner, params byte[] pos);
        public delegate void GateRemovedEffect(GateCard target, byte owner, params byte[] pos);
        public delegate void GateOpenEffect(GateCard target);
        public delegate void BattlesStartedEffect();
        public delegate void BattlesOverEffect();
        public delegate void TurnStartedEffect();
        public delegate void TurnAboutToEndEffect();
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
        public event GateOpenEffect GateOpen;
        public event BattlesStartedEffect BattlesStarted;
        public event BattlesOverEffect BattlesOver;
        public event TurnStartedEffect TurnStarted;
        public event TurnAboutToEndEffect TurnAboutToEnd;
        public event TurnEndEffect TurnEnd;

        public void OnBakuganBoosted(Bakugan target, Boost boost, object source) =>
            BakuganBoosted?.Invoke(target, boost, source);
        public void OnBakuganMoved(Bakugan target, IBakuganContainer pos) =>
            BakuganMoved?.Invoke(target, pos);
        public void OnBakuganAdded(Bakugan target, byte owner, IBakuganContainer pos) =>
            BakuganAdded?.Invoke(target, owner, pos);
        public void OnBakuganThrown(Bakugan target, byte owner, IBakuganContainer pos)
        {
            BakuganAdded?.Invoke(target, owner, pos);
            BakuganThrown?.Invoke(target, owner, pos);
        }
        public void OnBakuganPlacedFromGrave(Bakugan target, byte owner, IBakuganContainer pos)
        {
            BakuganPlacedFromGrave?.Invoke(target, owner, pos);
            BakuganAdded?.Invoke(target, owner, pos);
        }
        public void OnBakuganReturned(Bakugan target, byte owner) =>
            BakuganReturned?.Invoke(target, owner);
        public void OnBakuganDestroyed(Bakugan target, byte owner) =>
            BakuganDestroyed?.Invoke(target, owner);
        public void OnBakuganRevived(Bakugan target, byte owner) =>
            BakuganRevived?.Invoke(target, owner);
        public void OnGateAdded(GateCard target, byte owner, params byte[] pos) =>
            GateAdded?.Invoke(target, owner, pos);
        public void OnGateRemoved(GateCard target, byte owner, params byte[] pos) =>
            GateRemoved?.Invoke(target, owner, pos);
        public void OnGateOpen(GateCard target) =>
            GateOpen?.Invoke(target);

        public Action[] OnAnswer;
        public Action NextStep;

        public bool Started = false;
        bool Over = false;
        public int Left = 0;

        public bool DontThrowTurnStartEvent = false;

        public Game(byte playerCount)
        {
            Field = new GateCard[2, 3];
            PlayerCount = playerCount;
            NewEvents = new List<JObject>[playerCount];
            OnAnswer = new Action[playerCount];
            Players = new();
            IncomingSelection = new JObject[playerCount];
            for (byte i = 0; i < playerCount; i++)
            {
                NewEvents[i] = new List<JObject>();
            }

            NextStep = ContinueGame;
        }

        public int AddPlayer(JObject deck, long UUID, string playerName, byte avatar)
        {
            Players.Add(Player.FromJson(loggedPlayers, loggedPlayers, deck, this, playerName, avatar));
            UUIDToPid.Add(UUID, loggedPlayers);
            loggedPlayers++;
            return loggedPlayers - 1;
        }

        public void AddSpectator(long uuid)
        {
            if (!Spectators.Contains(uuid))
            {
                SpectatorEvents.Add(uuid, []);
                Spectators.Add(uuid);
            }
            else
            {
                SpectatorEvents[uuid].Clear();
            }

            if (Players.Count == PlayerCount)
                SpectatorEvents[uuid].Add(new()
                {
                    ["Type"] = "InitGameState",
                    ["PlayerNames"] = new JArray(Players.Select(x => x.DisplayName)),
                    ["PlayerColors"] = new JArray(Players.Select(x => x.PlayerColor)),
                    ["PlayerAvas"] = new JArray(Players.Select(x => x.Avatar)),
                    ["FieldGates"] = new JArray(GateIndex.Where(x => x.OnField).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["PosX"] = x.Position.X,
                        ["PosY"] = x.Position.Y,
                        ["IsOpen"] = x.IsOpen,
                        ["GateData"] = new JObject
                        {
                            ["CardType"] = x.IsOpen ? x.TypeId : -2
                        }
                    })),
                    ["FieldBakugan"] = new JArray(BakuganIndex.Where(x => x.OnField()).Select(x => new JObject
                    {
                        ["BID"] = x.BID,
                        ["PosX"] = (x.Position as GateCard).Position.X,
                        ["PosY"] = (x.Position as GateCard).Position.Y,
                        ["Type"] = (int)x.Type,
                        ["Attribute"] = (int)x.BaseAttribute,
                        ["Treatment"] = (int)x.Treatment,
                        ["Power"] = x.Power
                    })),
                    ["Actives"] = new JArray(ActiveZone.Where(x => x is not GateCard).Select(x => new JObject
                    {
                        ["EID"] = x.EffectId,
                        ["ActiveType"] = x is AbilityCard ? "Ability" : "Marker",
                        ["Kind"] = (int)x.Kind,
                        ["Type"] = x.TypeId,
                        ["User"] = x.User.BID,
                        ["Owner"] = x.Owner.Id,
                        ["IsCopy"] = false
                    })),
                    ["GraveBakugan"] = new JArray(BakuganIndex.Where(x => x.InGrave()).Select(x => new JObject
                    {
                        ["BID"] = x.BID,
                        ["Owner"] = x.Owner.Id,
                        ["BakuganType"] = (int)x.Type,
                        ["Attribute"] = (int)x.BaseAttribute,
                        ["Treatment"] = (int)x.Treatment,
                        ["IsPartner"] = x.IsPartner,
                        ["Power"] = x.Power
                    })),
                    ["GraveAbilities"] = new JArray(Players.SelectMany(x => x.AbilityGrave).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["Kind"] = (int)x.Kind,
                        ["CardType"] = x.TypeId
                    })),
                    ["GraveGates"] = new JArray(Players.SelectMany(x => x.GateGrave).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["CardType"] = x.TypeId
                    }))
                });
        }

        public byte GetPid(long UUID)
        {
            return UUIDToPid[UUID];
        }

        public void ThrowEvent(JObject @event, params int[] exclude)
        {
            Console.WriteLine(@event);
            for (int i = 0; i < PlayerCount; i++)
                if (!exclude.Contains(i))
                    NewEvents[i].Add(@event);
            foreach (var spectator in Spectators)
                SpectatorEvents[spectator].Add(@event);
        }

        public JArray GetEvents(int player)
        {
            checkDeadTimers[player].Stop();
            checkDeadTimers[player].Start();
            JArray toReturn;
            toReturn = [.. NewEvents[player]];
            NewEvents[player].Clear();

            return toReturn;
        }

        public JArray GetSpectatorEvents(long uuid)
        {
            JArray toReturn;
            toReturn = [.. SpectatorEvents[uuid]];
            SpectatorEvents[uuid].Clear();

            return toReturn;
        }

        System.Timers.Timer[] checkDeadTimers;

        public void Initiate()
        {
            checkDeadTimers = new System.Timers.Timer[PlayerCount];
            for (int i = 0; i < PlayerCount; i++)
            {
                var checkDeadTimer = new System.Timers.Timer()
                {
                    Enabled = false,
                    AutoReset = false,
                    Interval = 10000
                };
                checkDeadTimer.Elapsed += (object? sender, ElapsedEventArgs e) => Conclude(i);
                checkDeadTimer.Start();
                checkDeadTimers[i] = checkDeadTimer;
            }
            Started = true;
            SideCount = (byte)Players.Select(x => x.SideID).Distinct().Count();
            foreach (var e in SpectatorEvents.Values)
                e.Add(new()
                {
                    ["Type"] = "InitGameState",
                    ["PlayerNames"] = new JArray(Players.Select(x => x.DisplayName)),
                    ["PlayerColors"] = new JArray(Players.Select(x => x.PlayerColor)),
                    ["PlayerAvas"] = new JArray(Players.Select(x => x.Avatar)),
                    ["FieldGates"] = new JArray(GateIndex.Where(x => x.OnField).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["PosX"] = x.Position.X,
                        ["PosY"] = x.Position.Y,
                        ["IsOpen"] = x.IsOpen,
                        ["GateData"] = new JObject
                        {
                            ["CardType"] = x.IsOpen ? x.TypeId : -2
                        }
                    })),
                    ["FieldBakugan"] = new JArray(BakuganIndex.Where(x => x.OnField()).Select(x => new JObject
                    {
                        ["BID"] = x.BID,
                        ["PosX"] = (x.Position as GateCard).Position.X,
                        ["PosY"] = (x.Position as GateCard).Position.Y,
                        ["Type"] = (int)x.Type,
                        ["Attribute"] = (int)x.BaseAttribute,
                        ["Treatment"] = (int)x.Treatment,
                        ["Power"] = x.Power
                    })),
                    ["Actives"] = new JArray(ActiveZone.Where(x => x is not GateCard).Select(x => new JObject
                    {
                        ["EID"] = x.EffectId,
                        ["ActiveType"] = x is AbilityCard ? "Ability" : "Marker",
                        ["Kind"] = (int)x.Kind,
                        ["Type"] = x.TypeId,
                        ["User"] = x.User.BID,
                        ["Owner"] = x.Owner.Id,
                        ["IsCopy"] = false
                    })),
                    ["GraveBakugan"] = new JArray(BakuganIndex.Where(x => x.InGrave()).Select(x => new JObject
                    {
                        ["BID"] = x.BID,
                        ["Owner"] = x.Owner.Id,
                        ["BakuganType"] = (int)x.Type,
                        ["Attribute"] = (int)x.BaseAttribute,
                        ["Treatment"] = (int)x.Treatment,
                        ["IsPartner"] = x.IsPartner,
                        ["Power"] = x.Power
                    })),
                    ["GraveAbilities"] = new JArray(Players.SelectMany(x => x.AbilityGrave).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["Kind"] = (int)x.Kind,
                        ["CardType"] = x.TypeId
                    })),
                    ["GraveGates"] = new JArray(Players.SelectMany(x => x.GateGrave).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["CardType"] = x.TypeId
                    }))
                });

            TurnPlayer = (byte)new Random().Next(Players.Count);
            ActivePlayer = TurnPlayer;

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                JArray gates = new();

                for (int j = 0; j < player.GateHand.Count; j++)
                {
                    int type = player.GateHand[j].TypeId;
                    switch (type)
                    {
                        //case 0:
                        //    gates.Add(new JObject { { "Type", type }, { "Attribute", (int)((NormalGate)p.GateHand[j]).Attribute }, { "Power", ((NormalGate)p.GateHand[j]).Power } });
                        //    break;
                        //case 4:
                        //    gates.Add(new JObject { { "Type", type }, { "Attribute", (int)((AttributeHazard)p.GateHand[j]).Attribute } });
                        //    break;
                        default:
                            gates.Add(new JObject { { "Type", type }, { "CID", player.GateHand[j].CardId } });
                            break;
                    }
                }
                NewEvents[i].Add(new JObject
                {
                    ["Type"] = "InitializeHand",
                    ["Bakugans"] = new JArray(player.Bakugans.Select(b => new JObject
                    {
                        ["BID"] = b.BID,
                        ["BakuganType"] = (int)b.Type,
                        ["Attribute"] = (int)b.MainAttribute,
                        ["Treatment"] = (int)b.Treatment,
                        ["Power"] = b.Power,
                        ["IsPartner"] = b.IsPartner
                    })),
                    ["Abilities"] = new JArray(player.AbilityHand.Select(a => new JObject
                    {
                        ["CID"] = a.CardId,
                        ["Type"] = a.TypeId,
                        ["Kind"] = (int)a.Kind
                    })),
                    ["Gates"] = new JArray(player.GateHand.Select(g => new JObject
                    {
                        ["CID"] = g.CardId,
                        ["Type"] = g.TypeId
                    }))
                });
                NewEvents[i].Add(new JObject { { "Type", "PickGateEvent" }, { "Prompt", "pick_gate_start" }, { "Gates", gates } });
                ThrowEvent(new JObject { { "Type", "PlayerGatesColors" }, { "Player", i }, { "Color", Players[i].PlayerColor } });
            }

            for (int i = 0; i < PlayerCount; i++)
                OnAnswer[i] = () =>
                {
                    if (IncomingSelection.Contains(null)) return;
                    for (byte j = 0; j < IncomingSelection.Length; j++)
                    {
                        dynamic selection = IncomingSelection[j];
                        int id = (int)selection["gate"];

                        ThrowEvent(new()
                        {
                            ["Type"] = "GateRemovedFromHand",
                            ["CardType"] = GateIndex[(byte)selection["gate"]].TypeId,
                            ["CID"] = GateIndex[(byte)selection["gate"]].CardId,
                            ["Owner"] = j
                        });
                        GateIndex[(byte)selection["gate"]].Set(j, 1);
                    }

                    ThrowEvent(new()
                    {
                        { "Type", "PhaseChange" },
                        { "Phase", "TurnStart" }
                    });
                    ThrowEvent(new()
                    {
                        { "Type", "PhaseChange" },
                        { "Phase", "Main" }
                    });
                    ThrowEvent(new JObject { { "Type", "NewTurnEvent" }, { "TurnPlayer", TurnPlayer }, { "TurnNumber", currentTurn } });
                    ThrowEvent(new JObject { { "Type", "PlayerTurnStart" }, { "PID", ActivePlayer } });
                    Started = true;
                };
        }

        private void Conclude(int player)
        {
            checkDeadTimers[player].Stop();
            ThrowEvent(new JObject { { "Type", "GameOver" }, { "Draw", false }, { "Victor", Players.First(x => x.Id != player).Id } });
            Over = true;
        }

        public bool doNotMakeStep = false;

        public void GameStep(JObject selection)
        {
            string moveType = selection["Type"].ToString();

            DontThrowTurnStartEvent = false;
            if (moveType != "pass")
                playersPassed.Clear();
            switch (moveType)
            {
                case "throw":
                    if (Field[(int)selection["posX"], (int)selection["posY"]] is GateCard gateSelection)
                    {
                        Players[TurnPlayer].HadThrownBakugan = true;
                        BakuganIndex[(int)selection["bakugan"]].Throw(gateSelection);
                    }
                    else
                    {
                        NewEvents[TurnPlayer].Add(new JObject
                        {
                            ["Type"] = "InvalidAction"
                        });
                    }
                    break;
                case "set":
                    (byte X, byte Y) posSelection = ((byte)selection["posX"], (byte)selection["posY"]);

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

                        var id = (byte)selection["gate"];
                        ThrowEvent(new()
                        {
                            ["Type"] = "GateRemovedFromHand",
                            ["CardType"] = GateIndex[id].TypeId,
                            ["CID"] = id,
                            ["Owner"] = GateIndex[id].Owner.Id
                        });

                        GateIndex[id].Set(posSelection.X, posSelection.Y);
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
                        CardChain.Add(AbilityIndex[abilitySelection]);
                        AbilityIndex[abilitySelection].EffectId = NextEffectId++;
                        ActiveZone.Add(AbilityIndex[abilitySelection]);
                        Players[TurnPlayer].AbilityHand.Remove(AbilityIndex[abilitySelection]);
                        ThrowEvent(new()
                        {
                            ["Type"] = "AbilityRemovedFromHand",
                            ["Kind"] = (int)AbilityIndex[abilitySelection].Kind,
                            ["CardType"] = AbilityIndex[abilitySelection].TypeId,
                            ["CID"] = AbilityIndex[abilitySelection].CardId,
                            ["Owner"] = AbilityIndex[abilitySelection].Owner.Id
                        });

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
                    playersPassed.Add(Players[ActivePlayer]);

                    var battlingPlayers = Players.Where(x => x.HasBattlingBakugan());
                    var allBattlingPlayersPassed = true;
                    foreach (var player in battlingPlayers)
                    {
                        if (!playersPassed.Contains(player)) allBattlingPlayersPassed = false;
                    }
                    if (allBattlingPlayersPassed)
                    {
                        playersPassed.Clear();
                        foreach (var g in Field.Cast<GateCard?>())
                            if (g?.ActiveBattle == true)
                                g.DetermineWinner();

                        int loser = -1;
                        foreach (var p in Players)
                            if (!p.BakuganOwned.Any(x => !x.Defeated))
                            {
                                loser = p.Id;
                                break;
                            }

                        if (loser != -1)
                        {
                            ThrowEvent(new JObject { { "Type", "GameOver" }, { "Draw", false }, { "Victor", Players.First(x => x.Id != loser).Id } });
                            Over = true;
                            break;
                        }

                        WindowSuggested = false;
                        playersPassedCount = 0;
                    }

                    break;
                case "end":
                    if (Players[TurnPlayer].Bakugans.Count > 0 && !Players[TurnPlayer].HadThrownBakugan)
                    {
                        NewEvents[TurnPlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }

                    if (Players[TurnPlayer].CanEndTurn())
                    {
                        EndTurn();
                        DontThrowTurnStartEvent = true;
                    }
                    else
                        NewEvents[TurnPlayer].Add(new JObject { { "Type", "InvalidAction" } });

                    break;
                case "draw":
                    var toSuggestDraw = Players.First(x => x.Id != ActivePlayer).Id;
                    NewEvents[toSuggestDraw].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("INFO_SUGGESTDRAW")));
                    OnAnswer[toSuggestDraw] = () =>
                    {
                        bool answer = (bool)IncomingSelection[toSuggestDraw]["array"][0]["answer"];
                        if (answer)
                        {
                            ThrowEvent(new JObject { { "Type", "GameOver" }, { "Draw", true } });
                            Over = true;
                        }
                        else
                        {
                            NextStep();
                        }
                    };
                    doNotMakeStep = true;
                    break;
            }
            if (isBattleGoing)
            {
                var startPlayer = ActivePlayer;
                while (true)
                {
                    ActivePlayer++;
                    if (ActivePlayer >= PlayerCount) ActivePlayer = 0;
                    if (Players[ActivePlayer].HasBattlingBakugan())
                        break;
                    if (startPlayer == ActivePlayer)
                    {
                        break;
                    }
                }
            }
            if (Over) return;
            if (!DontThrowTurnStartEvent)
                NextStep();
        }

        bool WindowSuggested = false;
        public void ContinueGame()
        {
            foreach (var gate in GateIndex.Where(x => x.OnField && !BattlesToStart.Contains(x)))
                gate.CheckBattles();
            int loser = -1;
            foreach (var p in Players)
                if (!p.BakuganOwned.Any(x => !x.Defeated))
                {
                    loser = p.Id;
                    break;
                }

            if (loser != -1)
            {
                ThrowEvent(new JObject { { "Type", "GameOver" }, { "Draw", false }, { "Victor", Players.First(x => x.Id != loser).Id } });
                Over = true;
                return;
            }

            if (AutoGatesToOpen.Count != 0)
            {
                NextStep = ProcessAutoGate;
                ProcessAutoGate();
                return;
            }

            Console.WriteLine("Battles to start: " + BattlesToStart.Count.ToString());
            if (BattlesToStart.Count != 0)
            {
                Console.WriteLine("Window suggested: " + WindowSuggested.ToString());
                if (!WindowSuggested)
                {
                    WindowSuggested = true;
                    SuggestWindow(ActivationWindow.BattleStart, ActivePlayer, ActivePlayer);
                }
                else
                {
                    WindowSuggested = false;
                    BattlesStarted?.Invoke();
                    foreach (GateCard x in BattlesToStart)
                    {
                        if (x.Freezing.Count != 0) continue;
                        NextStep = () => { };
                        x.StartBattle();
                        NextStep = ContinueGame;
                    }
                    BattlesToStart.Clear();
                    Console.WriteLine("Battles to start cleared");
                    ContinueGame();
                }
            }
            else if (BattlesToEnd.Count != 0)
            {
                if (!WindowSuggested)
                {
                    WindowSuggested = true;
                    if (BattlesToEnd.Any(x => !x.CheckBattles()))
                    {
                        SuggestWindow(ActivationWindow.BattleEnd, ActivePlayer, ActivePlayer);
                    }
                    else
                        NextStep();
                }
                else
                {
                    WindowSuggested = false;
                    BattlesOver?.Invoke();
                    BattlesToEnd.ForEach(x =>
                    {
                        if (!x.CheckBattles())
                            x.Dispose();
                    });
                    BattlesToEnd.Clear();

                    // Check if new battles have started
                    if (BattlesToStart.Count != 0)
                    {
                        NextStep();
                    }
                    else
                    {
                        // Ensure the turn ends after resolving battles
                        EndTurn();
                    }
                }
            }
            else
            {
                CurrentWindow = ActivationWindow.Normal;
                doNotMakeStep = false;

                if (isBattleGoing && !Players[ActivePlayer].HasBattlingBakugan())
                {
                    var startPlayer = ActivePlayer;
                    while (true)
                    {
                        ActivePlayer++;
                        if (ActivePlayer >= PlayerCount) ActivePlayer = 0;
                        if (Players[ActivePlayer].HasBattlingBakugan())
                            break;
                        if (startPlayer == ActivePlayer)
                        {
                            Console.WriteLine("WARNING! NO PLAYER CAN MAKE MOVE!");
                            break;
                        }
                    }
                }

                foreach (var playerEvents in NewEvents)
                    playerEvents.Add(new JObject { { "Type", "PlayerTurnStart" }, { "PID", ActivePlayer } });
            }
        }

        public void ProcessAutoGate()
        {
            if (AutoGatesToOpen.Count == 0)
            {
                NextStep = ContinueGame;
                CardChain.Reverse();
                var toSend = CardChain[0] as GateCard;
                CheckChain(toSend.Owner, toSend);
            }
            else
            {
                var gateToProcess = AutoGatesToOpen[0];
                AutoGatesToOpen.RemoveAt(0);
                CardChain.Add(gateToProcess);
                ActiveZone.Add(gateToProcess);
                gateToProcess.Open();
            }
        }
        public void EndTurn()
        {
            playersPassed.Clear();

            TurnAboutToEnd?.Invoke();

            if (isBattleGoing)
            {
                ActivePlayer = TurnPlayer;
                foreach (var e in NewEvents) e.Add(new JObject
                        {
                            { "Type", "PlayerTurnStart" },
                            { "PID", ActivePlayer }
                        });
            }
            else
            {
                NextStep = () =>
                {
                    ThrowEvent(new()
                    {
                        { "Type", "PhaseChange" },
                        { "Phase", "TurnEnd" }
                    });
                    NextStep = StartTurn;
                    SuggestWindow(ActivationWindow.TurnEnd, ActivePlayer, ActivePlayer);
                };
                TurnEnd?.Invoke();
                if (!isBattleGoing)
                    NextStep();

            }
        }

        public void StartTurn()
        {
            if (++TurnPlayer == PlayerCount) TurnPlayer = 0;
            ActivePlayer = TurnPlayer;

            Players[TurnPlayer].HadSetGate = false;
            Players[TurnPlayer].HadThrownBakugan = false;
            foreach (Player player in Players)
                player.HadUsedCounter = false;

            if (!BakuganIndex.Any(x => x.InHand()))
            {
                ThrowEvent(new()
                {
                    { "Type", "GateClosing" }
                });
                foreach (var bakugan in BakuganIndex.Where(x => x.OnField()))
                    if (bakugan.Position is GateCard positionGate)
                        bakugan.ToHand(positionGate.EnterOrder);
                GateSetList[^1].Dispose();
                foreach (var gate in GateIndex.Where(x => x.OnField))
                    gate.Retract();
            }

            currentTurn++;
            ThrowEvent(new JObject { { "Type", "NewTurnEvent" }, { "TurnPlayer", TurnPlayer }, { "TurnNumber", currentTurn } });

            if (Field.Cast<GateCard?>().All(x => x is null) && Players.All(x => x.GateHand.Count == 0))
            {
                var gate = new NormalGate(GateIndex.Count, Players[TurnPlayer]);
                Players[TurnPlayer].GateHand.Add(gate);
                GateIndex.Add(gate);
                ThrowEvent(new JObject {
                    { "Type", "GateAddedToHand" },
                    { "Owner", TurnPlayer },
                    { "GateType", -1 },
                    { "CID", gate.CardId }
                });
            }

            ThrowEvent(new()
            {
                { "Type", "PhaseChange" },
                { "Phase", "TurnStart" }
            });
            TurnStarted?.Invoke();

            NextStep = () =>
            {
                ThrowEvent(new()
                {
                    { "Type", "PhaseChange" },
                    { "Phase", "Main" }
                });
                NextStep = ContinueGame;
                ContinueGame();
            };
            SuggestWindow(ActivationWindow.TurnStart, ActivePlayer, ActivePlayer);
        }

        public JObject GetPossibleMoves(int player)
        {
            JArray gateArray = new JArray();

            foreach (var gate in Players[player].SettableGates())
                switch (gate.TypeId)
                {
                    //case 0:
                    //    gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId }, { "Attribute", (int)((NormalGate)gate).Attribute }, { "Power", ((NormalGate)gate).Power } });
                    //    break;
                    //case 4:
                    //    gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId }, { "Attribute", (int)((AttributeHazard)gate).Attribute } });
                    //    break;
                    default:
                        gateArray.Add(new JObject { { "CID", gate.CardId }, { "Type", gate.TypeId } });
                        break;
                }

            JArray bakuganArray = new JArray();

            foreach (var bakugan in Players[player].ThrowableBakugan())
                bakuganArray.Add(new JObject { { "BID", bakugan.BID }, { "Type", (int)bakugan.Type }, { "Attribute", (int)bakugan.MainAttribute }, { "Treatment", (int)bakugan.Treatment }, { "IsPartner", bakugan.IsPartner }, { "Power", bakugan.Power } });

            JObject moves = new()
            {
                { "CanSetGate", Players[player].HasSettableGates() && !isBattleGoing },
                { "CanOpenGate", Players[player].HasOpenableGates() },
                { "CanThrowBakugan", !isBattleGoing && !Players[player].HadThrownBakugan && Players[player].HasThrowableBakugan() && GateIndex.Any(x=>x.OnField) },
                { "CanActivateAbility", Players[player].HasActivateableAbilities() && (Players[player].AbilityBlockers.Count == 0) },
                { "CanEndTurn", Players[player].CanEndTurn() },
                { "CanEndBattle", Players[player].HasBattlingBakugan() },

                { "IsASkip", !Players[player].HadThrownBakugan },
                { "IsAPass", isBattleGoing && playersPassed.Count < (Players.Count(x=>x.HasBattlingBakugan()) - 1) },

                { "SettableGates", gateArray },
                { "OpenableGates", new JArray(Players[player].OpenableGates().Select(x => new JObject { { "CID", x.CardId }, { "PosX", x.Position.X }, { "PosY", x.Position.Y } } )) },
                { "ThrowableBakugan", bakuganArray },
                { "ValidThrowPositions", new JArray(GateIndex.Where(x=>x.OnField && x.ThrowBlocking.Count == 0).Select(x => new JObject { { "CID", x.CardId }, { "PosX", x.Position.X }, { "PosY", x.Position.Y } })) },
                { "ActivateableAbilities", new JArray(Players[player].AbilityHand.Select(x => new JObject { { "cid", x.CardId }, { "Type", x.TypeId }, { "Kind", (int)x.Kind }, { "CanActivate", x.IsActivateable() } })) }
            };
            return moves;
        }

        public void CheckChain(Player player, AbilityCard ability, Bakugan user)
        {
            //if (!player.HadUsedFusion && player.HasActivateableFusionAbilities(user))
            //    SuggestFusion(player, ability, user);
            //else 
            if (Players.Any(x => !x.HadUsedCounter && x.HasActivateableAbilities()))
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

        public void CheckChain(Player player, GateCard gate)
        {
            if (Players.Any(x => !x.HadUsedCounter && x.HasActivateableAbilities()))
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
                SuggestCounter(Players[next], gate, player);
            }
            else ResolveChain();
        }

        public void SuggestWindow(ActivationWindow window, int startingPlayer, int player)
        {
            CurrentWindow = window;
            var currentPlayer = Players[player];

            if (currentPlayer.HasActivateableAbilities())
            {
                OnAnswer[player] = () => CheckWindow(startingPlayer, player);
                NewEvents[player].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("INFO_" + window.ToString().ToUpper() + "WINDOWPROMPT")));
            }
            else
            {
                if (++player >= PlayerCount) player = 0;

                if (player == startingPlayer)
                {
                    NextStep();
                }
                else SuggestWindow(window, startingPlayer, player);
            }
        }

        public void CheckWindow(int startingPlayer, int player)
        {
            if ((bool)IncomingSelection[player]["array"][0]["answer"])
            {
                OnAnswer[player] = () => ResolveWindow(Players[player]);
                NewEvents[player].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection("INFO_" + CurrentWindow.ToString().ToUpper() + "WINDOWSELECTION", Players[player].AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
            }
            else
            {
                if (++player >= PlayerCount) player = 0;

                if (player == startingPlayer) NextStep();
                else SuggestWindow(CurrentWindow, startingPlayer, player);
            }
        }

        public void ResolveWindow(Player player)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateable())
            {
                CardChain.Add(AbilityIndex[id]);
                AbilityIndex[id].EffectId = NextEffectId++;
                ActiveZone.Add(AbilityIndex[id]);
                player.AbilityHand.Remove(AbilityIndex[id]);

                ThrowEvent(new()
                {
                    ["Type"] = "AbilityRemovedFromHand",
                    ["Kind"] = (int)AbilityIndex[id].Kind,
                    ["CardType"] = AbilityIndex[id].TypeId,
                    ["CID"] = AbilityIndex[id].CardId,
                    ["Owner"] = AbilityIndex[id].Owner.Id
                });

                AbilityIndex[id].Setup(false);
            }
        }

        public void SuggestCounter(Player player, IActive card, Player user)
        {
            OnAnswer[player.Id] = () => CheckCounter(player, card, user);
            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.CounterSelectionEvent(user.Id, card.TypeId, (int)card.Kind)));
        }

        public void CheckCounter(Player player, IActive card, Player user)
        {
            if (!(bool)IncomingSelection[player.Id]["array"][0]["answer"])
            {
                int next = player.Id + 1;
                if (next == PlayerCount) next = 0;
                if (next == user.Id) ResolveChain();
                else SuggestCounter(Players[next], card, user);
            }
            else
            {
                player.HadUsedCounter = true;
                OnAnswer[player.Id] = () => ResolveCounter(player);

                NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection("CounterSelection", player.AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
            }
        }

        public void ResolveCounter(Player player)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateableCounter())
            {
                CardChain.Add(AbilityIndex[id]);
                AbilityIndex[id].EffectId = NextEffectId++;
                ActiveZone.Add(AbilityIndex[id]);
                player.AbilityHand.Remove(AbilityIndex[id]);

                ThrowEvent(new()
                {
                    ["Type"] = "AbilityRemovedFromHand",
                    ["Kind"] = (int)AbilityIndex[id].Kind,
                    ["CardType"] = AbilityIndex[id].TypeId,
                    ["CID"] = AbilityIndex[id].CardId,
                    ["Owner"] = AbilityIndex[id].Owner.Id
                });

                AbilityIndex[id].Setup(true);
            }
        }

        public void SuggestFusion(Player player, AbilityCard ability, Bakugan user)
        {
            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("FUSIONPROMPT")));

            OnAnswer[player.Id] = () => CheckFusion(player, ability, user);
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

            OnAnswer[player.Id] = () => ResolveFusion(player, ability, user);

            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(EventBuilder.AbilitySelection("FusionSelection", player.AbilityHand.Where(x => x.IsActivateableByBakugan(user)).ToArray())));
        }

        public void ResolveFusion(Player player, AbilityCard ability, Bakugan user)
        {
            int id = (int)IncomingSelection[player.Id]["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateableByBakugan(user))
            {
                CardChain.Insert(CardChain.IndexOf(ability) + 1, AbilityIndex[id]);
                AbilityIndex[id].EffectId = NextEffectId++;
                ActiveZone.Add(AbilityIndex[id]);
                player.AbilityHand.Remove(AbilityIndex[id]);

                //AbilityIndex[id].SetupFusion(ability, user);
            }
        }

        public bool ExecutingChain = false;
        public void ResolveChain()
        {
            ExecutingChain = true;

            CardChain.Reverse();
            var chain = new List<IChainable>(CardChain);
            CardChain.Clear();
            chain.ForEach(card => card.Resolve());

            ExecutingChain = false;

            NextStep();
        }
    }
}
