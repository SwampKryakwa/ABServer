using AB_Server.Abilities;
using AB_Server.Gates;
using AB_Server.Gates.SpecialGates;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        //Static data
        static readonly (byte X, byte Y)[] FirstCardPositions =
        [
            (1, 2),
            (2, 2),
            (1, 3),
            (2, 3)
        ];

        //Player data
        public Dictionary<long, int> UidToPid = [];
        public byte PlayerCount;
        public byte TeamCount;
        public Player[] Players;
        public List<long> Spectators = [];

        //Indexes
        public List<Bakugan> BakuganIndex = [];
        public List<GateCard> GateIndex = [];
        public List<AbilityCard> AbilityIndex = [];

        //Event containers
        public List<JObject>[] NewEvents;
        public Dictionary<long, List<JObject>> SpectatorEvents = [];

        //Game field
        public GateCard?[,] Field = new GateCard[4, 5];
        public List<GateCard> GateSetList = [];
        public List<IActive> ActiveZone = [];
        public Stack<IChainable> CardChain = [];

        public GateCard? GetGateByCoord(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Field.GetLength(0) || Y >= Field.GetLength(1)) return null;
            return Field[X, Y];
        }

        //Game state
        bool Over = false;
        public int turnNumber = 0;
        public byte TurnPlayer;
        public byte ActivePlayer;
        public bool isBattleGoing { get => GateIndex.Any(x => x.OnField && x.IsBattleGoing); }
        public ActivationWindow CurrentWindow = ActivationWindow.Normal;
        public readonly List<GateCard> BattlesToEnd = [];
        public readonly List<GateCard> AutoGatesToOpen = [];
        readonly List<Player> playersPassed = [];
        public byte PlayersLeft = 0;

        //Communication with the players
        public dynamic?[] PlayerAnswers;
        public Action[] OnAnswer;
        public Action[] OnCancel;

        //Long-range battles stuff
        public bool LongRangeBattleGoing;
        public Bakugan? Attacker;
        public Bakugan[]? Targets;
        public Action? OnLongRangeBattleOver;

        //Game flow
        public Action NextStep;

        //Other data
        byte playersCreated = 0;
        byte playersRegistered = 0;
        public int NextEffectId = 0;
        public bool DoNotMakeStep = false;

        //All the event types in the game
        public delegate void BakuganBoostedEffect(Bakugan target, Boost boost, object source);
        public delegate void BakuganPowerResetEffect(Bakugan bakugan);
        public delegate void BakuganMovedEffect(Bakugan target, IBakuganContainer pos);
        public delegate void BakuganReturnedEffect(Bakugan target, byte owner);
        public delegate void BakuganDestroyedEffect(Bakugan target, byte owner);
        public delegate void BakuganRevivedEffect(Bakugan target, byte owner);
        public delegate void BakuganThrownEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganAddedEffect(Bakugan target, byte owner, IBakuganContainer pos);
        public delegate void BakuganPlacedFromDropEffect(Bakugan target, byte owner, IBakuganContainer pos);
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
        public event BakuganPlacedFromDropEffect BakuganPlacedFromDrop;
        public event GateAddedEffect GateAdded;
        public event GateRemovedEffect GateRemoved;
        public event GateOpenEffect GateOpen;
        public event BattlesStartedEffect BattlesStarted;
        public event BattlesOverEffect BattlesOver;
        public event TurnStartedEffect TurnStarted;
        public event TurnAboutToEndEffect TurnAboutToEnd;
        public event TurnEndEffect TurnEnd;

        //Access to the game's events
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
        public void OnBakuganPlacedFromDrop(Bakugan target, byte owner, IBakuganContainer pos)
        {
            BakuganPlacedFromDrop?.Invoke(target, owner, pos);
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

        public Game(byte playerCount, byte sideCount)
        {
            PlayerCount = playerCount;
            Players = new Player[playerCount];
            NewEvents = new List<JObject>[playerCount];
            PlayerAnswers = new JObject[playerCount];
            OnAnswer = new Action[playerCount];
            OnCancel = new Action[playerCount];
            for (byte i = 0; i < playerCount; i++)
                NewEvents[i] = [];
            TeamCount = sideCount;
        }

        public int CreatePlayer(Game game, string userName, byte team, long uuid)
        {
            Players[playersCreated] = new Player(game, playersCreated, team, userName);
            UidToPid.Add(uuid, playersCreated);
            return playersCreated++;
        }

        public bool RegisterPlayer(byte playerId, JObject deck, byte ava)
        {
            Players[playerId].Avatar = ava;
            Players[playerId].ProvideDeck(deck);
            return ++playersRegistered == PlayerCount;
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
                ["GraveBakugan"] = new JArray(BakuganIndex.Where(x => x.InDrop()).Select(x => new JObject
                {
                    ["BID"] = x.BID,
                    ["Owner"] = x.Owner.Id,
                    ["BakuganType"] = (int)x.Type,
                    ["Attribute"] = (int)x.BaseAttribute,
                    ["Treatment"] = (int)x.Treatment,
                    ["IsPartner"] = x.IsPartner,
                    ["Power"] = x.Power
                })),
                ["GraveAbilities"] = new JArray(Players.SelectMany(x => x.AbilityDrop).Select(x => new JObject
                {
                    ["CID"] = x.CardId,
                    ["Owner"] = x.Owner.Id,
                    ["Kind"] = (int)x.Kind,
                    ["CardType"] = x.TypeId
                })),
                ["GraveGates"] = new JArray(Players.SelectMany(x => x.GateDrop).Select(x => new JObject
                {
                    ["CID"] = x.CardId,
                    ["Owner"] = x.Owner.Id,
                    ["CardType"] = x.TypeId
                }))
            });
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

        public void StartGame()
        {
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
                    ["GraveBakugan"] = new JArray(BakuganIndex.Where(x => x.InDrop()).Select(x => new JObject
                    {
                        ["BID"] = x.BID,
                        ["Owner"] = x.Owner.Id,
                        ["BakuganType"] = (int)x.Type,
                        ["Attribute"] = (int)x.BaseAttribute,
                        ["Treatment"] = (int)x.Treatment,
                        ["IsPartner"] = x.IsPartner,
                        ["Power"] = x.Power
                    })),
                    ["GraveAbilities"] = new JArray(Players.SelectMany(x => x.AbilityDrop).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["Kind"] = (int)x.Kind,
                        ["CardType"] = x.TypeId
                    })),
                    ["GraveGates"] = new JArray(Players.SelectMany(x => x.GateDrop).Select(x => new JObject
                    {
                        ["CID"] = x.CardId,
                        ["Owner"] = x.Owner.Id,
                        ["CardType"] = x.TypeId
                    }))
                });

            TurnPlayer = (byte)new Random().Next(Players.Length);
            ActivePlayer = TurnPlayer;

            for (int i = 0; i < PlayerCount; i++)
            {
                var player = Players[i];

                JArray gates = [.. player.GateHand.Select(x => new JObject { ["Type"] = x.TypeId, ["CID"] = x.CardId })];

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
                    ["Gates"] = gates
                });
                NewEvents[i].Add(new JObject { ["Type"] = "PickGateEvent", ["Prompt"] = "pick_gate_start", ["Gates"] = gates });
                ThrowEvent(new JObject { ["Type"] = "PlayerGatesColors", ["Player"] = i, ["Color"] = Players[i].PlayerColor });
                OnAnswer[i] = () =>
                {
                    if (PlayerAnswers.Contains(null)) return;
                    for (byte j = 0; j < PlayerAnswers.Length; j++)
                    {
                        dynamic selection = PlayerAnswers[j];
                        int id = (int)selection["gate"];

                        ThrowEvent(new()
                        {
                            ["Type"] = "GateRemovedFromHand",
                            ["CardType"] = GateIndex[id].TypeId,
                            ["CID"] = GateIndex[id].CardId,
                            ["Owner"] = j
                        });
                        GateIndex[id].Set(FirstCardPositions[j].X, FirstCardPositions[j].Y);
                    }
                    StartTurn();
                };
            }
        }

        public void StartTurn()
        {
            shouldTurnEnd = false;
            ActivePlayer = TurnPlayer;

            //Reset flags
            Players[TurnPlayer].HadSetGate = false;
            Players[TurnPlayer].HadThrownBakugan = false;
            foreach (Player player in Players)
                player.HadUsedCounter = false;

            if (!BakuganIndex.Any(x => x.InHand()))
                CloseField();

            if (Field.Cast<GateCard?>().All(x => x is null) && Players.All(x => x.GateHand.Count == 0))
                ProvideNormalGates();

            turnNumber++;
            ThrowEvent(new JObject
            {
                ["Type"] = "NewTurnEvent",
                ["TurnPlayer"] = TurnPlayer,
                ["TurnNumber"] = turnNumber
            });

            ThrowEvent(new()
            {
                ["Type"] = "PhaseChange",
                ["Phase"] = "TurnStart"
            });
            TurnStarted?.Invoke();

            NextStep = () =>
            {
                CurrentWindow = ActivationWindow.Normal;
                ThrowEvent(new()
                {
                    ["Type"] = "PhaseChange",
                    ["Phase"] = "Main"
                });
                UpdateBattleStates();
            };
            SuggestWindow(ActivationWindow.TurnStart, ActivePlayer, ActivePlayer);
        }

        public void SuggestWindow(ActivationWindow window, int startingPlayer, int player)
        {
            CurrentWindow = window;
            var currentPlayer = Players[player];

            if (currentPlayer.HasActivateableAbilities())
            {
                OnAnswer[player] = () => CheckWindow(startingPlayer, player);
                NewEvents[player].Add(EventBuilder.SelectionBundler(false, EventBuilder.BoolSelectionEvent("INFO_" + window.ToString().ToUpper() + "WINDOWPROMPT")));
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
            if ((bool)PlayerAnswers[player]!["array"][0]["answer"])
            {
                OnAnswer[player] = () => ResolveWindow(Players[player]);
                NewEvents[player].Add(EventBuilder.SelectionBundler(false, EventBuilder.AbilitySelection("INFO_" + CurrentWindow.ToString().ToUpper() + "WINDOWSELECTION", Players[player].AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
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
            int id = (int)PlayerAnswers[player.Id]!["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateable())
            {
                AbilityIndex[id].Setup(false);
            }
        }

        public void CloseField()
        {
            ThrowEvent(new()
            {
                ["Type"] = "GateClosing"
            });
            Bakugan.MultiToHand(this, BakuganIndex.Where(x => x.OnField()), MoveSource.Game);
            GateSetList[^1].Dispose();
            foreach (var gate in GateIndex.Where(x => x.OnField))
                gate.Retract();
        }

        void ProvideNormalGates()
        {
            foreach (var player in Players)
            {
                var gate = new NormalGate(GateIndex.Count, player);
                player.GateHand.Add(gate);
                GateIndex.Add(gate);
                ThrowEvent(new JObject
                {
                    ["Type"] = "GateAddedToHand",
                    ["Owner"] = player.Id,
                    ["CardType"] = -1,
                    ["CID"] = gate.CardId
                });
            }
        }

        void CheckAnyBattlesToUpdateState()
        {
            if (Players.Where(x => x.Alive).Select(x => x.TeamId).Distinct().Count() == 1)
            {
                ThrowEvent(new JObject
                {
                    ["Type"] = "GameOver",
                    ["Victor"] = Players.Where(x => x.Alive).First().Id,
                    ["VictorTeam"] = Players.Where(x => x.Alive).First().TeamId
                });
            }
            else if (GateIndex.Any(x => x.BattleDeclaredOver) || GateIndex.Any(x => !x.BattleStarted && x.IsBattleGoing))
                UpdateBattleStates();
            else if (shouldTurnEnd)
                EndTurn();
            else
                ThrowMoveStart();
        }

        bool anyBattlesStarted;
        bool anyBattlesEnded;
        void UpdateBattleStates()
        {
            //Starting started battles
            anyBattlesStarted = false;
            anyBattlesEnded = false;
            foreach (var gate in GateIndex.Where(x => x.OnField))
            {
                if (gate.BattleStarted || !gate.IsBattleGoing) continue;
                gate.CheckAutoBattleStart();
                gate.BattleStarted = true;
                anyBattlesStarted = true;
                playersPassed.Clear();
            }

            //Cleaning up over battles
            foreach (var g in Field.Cast<GateCard?>().Where(x => x is GateCard gate && gate.BattleDeclaredOver))
            {
                anyBattlesEnded = true;
                g.DetermineWinner();
                g.CheckAutoBattleEnd();
            }

            OpenBattleStateChangeGates();
        }

        void OpenBattleStateChangeGates()
        {
            if (ActivePlayer == PlayerCount) ActivePlayer = 0;
            if (AutoGatesToOpen.Count == 0)
            {
                if (anyBattlesStarted)
                {
                    NextStep = () =>
                    {
                        NextStep = EndBattles;
                        if (anyBattlesEnded)
                            SuggestWindow(ActivationWindow.BattleEnd, TurnPlayer, TurnPlayer);
                        else
                            CheckAnyBattlesToUpdateState();
                    };
                    SuggestWindow(ActivationWindow.BattleStart, TurnPlayer, TurnPlayer);
                }
                else if (anyBattlesEnded)
                {
                    NextStep = EndBattles;
                    SuggestWindow(ActivationWindow.BattleEnd, TurnPlayer, TurnPlayer);
                }
                else
                    CheckAnyBattlesToUpdateState();
            }
            else
            {
                while (!AutoGatesToOpen.Any(x => x.Owner.Id == ActivePlayer))
                {
                    ActivePlayer++;
                    if (ActivePlayer == PlayerCount) ActivePlayer = 0;
                }

                NewEvents[ActivePlayer].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldGateSelection("INFO_OPENSTARTBATTLE", 0, 0, AutoGatesToOpen.Where(x => x.Owner.Id == ActivePlayer))
                ));
                OnAnswer[ActivePlayer] = () =>
                {
                    AutoGatesToOpen.Remove(GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]]);
                    CardChain.Push(GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]]);
                    GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]].Open();
                    ActivePlayer++;
                };
            }
        }

        public void ThrowMoveStart()
        {
            CurrentWindow = ActivationWindow.Normal;
            ThrowEvent(new JObject { ["Type"] = "PlayerTurnStart", ["PID"] = LongRangeBattleGoing ? Targets.First().Owner.Id : ActivePlayer });
        }

        public JObject GetPossibleMoves(int player)
        {
            Console.WriteLine($"Current window: " + CurrentWindow);
            JArray gateArray = new JArray();

            foreach (var gate in Players[player].SettableGates())
                gateArray.Add(new JObject { ["CID"] = gate.CardId, ["Type"] = gate.TypeId });

            JArray bakuganArray = new JArray();

            foreach (var bakugan in Players[player].ThrowableBakugan())
                bakuganArray.Add(new JObject { ["BID"] = bakugan.BID, ["Type"] = (int)bakugan.Type, ["Attribute"] = (int)bakugan.MainAttribute, ["Treatment"] = (int)bakugan.Treatment, ["IsPartner"] = bakugan.IsPartner, ["Power"] = bakugan.Power });

            JObject moves = new()
            {
                ["CanSetGate"] = Players[player].HasSettableGates() && !isBattleGoing,
                ["CanOpenGate"] = Players[player].HasOpenableGates() && Players[player].GateBlockers.Count == 0,
                ["CanThrowBakugan"] = !isBattleGoing && !Players[player].HadThrownBakugan && Players[player].HasThrowableBakugan() && GateIndex.Any(x => x.OnField),
                ["CanActivateAbility"] = Players[player].HasActivateableAbilities() && Players[player].AbilityBlockers.Count == 0,
                ["CanEndTurn"] = Players[player].CanEndTurn(),
                ["CanEndBattle"] = Players[player].HasBattlingBakugan(),

                ["IsASkip"] = !Players[player].HadThrownBakugan,
                ["IsAPass"] = (isBattleGoing && playersPassed.Count < (Players.Count(x => x.HasBattlingBakugan()) - 1)) || LongRangeBattleGoing,

                ["SettableGates"] = gateArray,
                ["OpenableGates"] = new JArray(Players[player].OpenableGates().Select(x => new JObject { ["CID"] = x.CardId, ["TypeId"] = x.TypeId, ["KindId"] = (int)x.Kind, ["PosX"] = x.Position.X, ["PosY"] = x.Position.Y })),
                ["ThrowableBakugan"] = bakuganArray,
                ["ValidThrowPositions"] = new JArray(GateIndex.Where(x => x.OnField && x.ThrowBlocking.Count == 0).Select(x => new JObject { { "CID", x.CardId }, { "Owner", x.Owner.Id }, { "PosX", x.Position.X }, { "PosY", x.Position.Y } })),
                ["ActivateableAbilities"] = new JArray(Players[player].AbilityHand.Select(ability => new JObject { ["cid"] = ability.CardId, ["Type"] = ability.TypeId, ["Kind"] = (int)ability.Kind, ["CanActivate"] = ability.IsActivateable(), ["PossibleUsers"] = new JArray(Players[player].BakuganOwned.Where(possibleUser => ability.BakuganIsValid(possibleUser)).Select(x => x.BID)) }))
            };
            return moves;
        }

        bool shouldTurnEnd = false;
        public void GameStep(JObject selection, int movePlayer)
        {
            if (LongRangeBattleGoing)
                NextStep = ResolveLongRangeBattle;
            string moveType = selection["Type"].ToString();

            bool DontThrowTurnStartEvent = false;
            if (moveType != "pass" && moveType != "draw")
                playersPassed.Clear();

            Console.WriteLine("Move type: " + moveType);
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
                        DoNotMakeStep = true;
                        AbilityIndex[abilitySelection].Setup(false);
                    }
                    break;
                case "cancel":
                    OnCancel[movePlayer]();

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
                        DoNotMakeStep = true;
                        gateToOpen.Open();
                    }
                    else
                        NewEvents[ActivePlayer].Add(new JObject { { "Type", "InvalidAction" } });

                    break;
                case "pass":
                    Console.WriteLine("Is long range battle going: " + LongRangeBattleGoing);
                    if (!isBattleGoing && !LongRangeBattleGoing)
                    {
                        NewEvents[ActivePlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }
                    if (LongRangeBattleGoing)
                    {
                        ResolveLongRangeBattle();
                        return;
                    }

                    playersPassed.Add(Players[ActivePlayer]);

                    var battlingPlayers = Players.Where(x => x.HasBattlingBakugan());
                    var allBattlingPlayersPassed = true;
                    Console.WriteLine("Players passed count: " + playersPassed.Count);
                    foreach (var player in battlingPlayers)
                    {
                        if (!playersPassed.Contains(player)) allBattlingPlayersPassed = false;
                    }
                    Console.WriteLine("All battling players passed: " + allBattlingPlayersPassed);
                    if (allBattlingPlayersPassed)
                    {
                        playersPassed.Clear();
                        foreach (var g in Field.Cast<GateCard?>().Where(x => x is GateCard gate && gate.IsBattleGoing))
                            g.BattleDeclaredOver = true;

                        shouldTurnEnd = true;
                        UpdateBattleStates();
                        return;
                    }

                    break;
                case "end":
                    if (!Players[TurnPlayer].CanEndTurn())
                    {
                        NewEvents[TurnPlayer].Add(new JObject { { "Type", "InvalidAction" } });
                        break;
                    }
                    else
                    {
                        EndTurn();
                        DontThrowTurnStartEvent = true;
                    }
                    break;
                case "draw":
                    var toSuggestDraw = Players.First(x => x.Id != ActivePlayer).Id;
                    NewEvents[toSuggestDraw].Add(EventBuilder.SelectionBundler(false, EventBuilder.BoolSelectionEvent("INFO_SUGGESTDRAW")));
                    OnAnswer[toSuggestDraw] = () =>
                    {
                        bool answer = (bool)PlayerAnswers[toSuggestDraw]!["array"][0]["answer"];
                        if (answer)
                        {
                            ThrowEvent(new JObject { { "Type", "GameOver" }, { "Draw", true } });
                            Over = true;
                        }
                        else
                        {
                            ThrowMoveStart();
                        }
                    };
                    DoNotMakeStep = true;
                    break;
            }
            if (isBattleGoing)
            {
                var startPlayer = ActivePlayer;
                while (true)
                {
                    if (moveType != "cancel")
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
                if (LongRangeBattleGoing)
                    ResolveLongRangeBattle();
                else
                    UpdateBattleStates();
        }

        void OpenEndBattleGates()
        {
            if (AutoGatesToOpen.Count == 0)
            {
                NextStep = EndBattles;
                if (GateIndex.Any(x => x.OnField && x.BattleOver))
                    SuggestWindow(ActivationWindow.BattleEnd, TurnPlayer, TurnPlayer);
                else
                    EndBattles();
            }
            else
            {
                while (!AutoGatesToOpen.Any(x => x.Owner.Id == ActivePlayer))
                {
                    ActivePlayer++;
                    if (ActivePlayer > PlayerCount) ActivePlayer = 0;
                }
                if (ActivePlayer > PlayerCount) ActivePlayer = 0;

                NewEvents[ActivePlayer].Add(EventBuilder.SelectionBundler(false,
                    EventBuilder.FieldGateSelection("INFO_OPENENDBATTLE", 0, 0, AutoGatesToOpen.Where(x => x.Owner.Id == ActivePlayer))
                ));
                OnAnswer[ActivePlayer] = () =>
                {
                    AutoGatesToOpen.Remove(GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]]);
                    GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]].Open();
                    CardChain.Push(GateIndex[(int)PlayerAnswers[ActivePlayer]!["array"][0]["gate"]]);
                    ActivePlayer++;
                };
            }
        }

        public void EndBattles()
        {
            foreach (var gate in GateIndex.Where(x => x.OnField && x.BattleOver))
            {
                gate.BattleOver = false;
                gate.Dispose();
            }
            BattlesOver?.Invoke();
            CheckAnyBattlesToUpdateState();
        }

        private void ResolveLongRangeBattle()
        {
            foreach (var target in Targets)
                if (target.Power < Attacker.Power && target.OnField() && Attacker.OnField())
                    target.DestroyOnField((target.Position as GateCard).EnterOrder);
            OnLongRangeBattleOver?.Invoke();
            LongRangeBattleGoing = false;
            ThrowMoveStart();
        }

        public void EndTurn()
        {
            playersPassed.Clear();

            TurnAboutToEnd?.Invoke();

            if (GateIndex.Any(x => x.OnField && x.IsBattleGoing && !x.BattleStarted))
            {
                CheckAnyBattlesToUpdateState();
                return;
            }

            NextStep = () =>
            {
                ThrowEvent(new()
                {
                    ["Type"] = "PhaseChange",
                    ["Phase"] = "TurnEnd"
                });
                NextStep = StartTurn;
                if (++TurnPlayer == PlayerCount) TurnPlayer = 0;
                ActivePlayer = TurnPlayer;
                StartTurn();
            };
            SuggestWindow(ActivationWindow.TurnEnd, ActivePlayer, ActivePlayer);
            TurnEnd?.Invoke();
        }

        public void CheckChain(Player player, AbilityCard ability, Bakugan user)
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

        public void SuggestCounter(Player player, IActive card, Player user)
        {
            OnAnswer[player.Id] = () => CheckCounter(player, card, user);
            NewEvents[player.Id].Add(EventBuilder.SelectionBundler(false, EventBuilder.CounterSelectionEvent(user.Id, card.TypeId, (int)card.Kind)));
        }

        public void CheckCounter(Player player, IActive card, Player user)
        {
            if (!(bool)PlayerAnswers[player.Id]!["array"][0]["answer"])
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

                NewEvents[player.Id].Add(EventBuilder.SelectionBundler(false, EventBuilder.AbilitySelection("CounterSelection", player.AbilityHand.Where(x => x.IsActivateableCounter()).ToArray())));
            }
        }

        public void ResolveCounter(Player player)
        {
            int id = (int)PlayerAnswers[player.Id]!["array"][0]["ability"];
            if (player.AbilityHand.Contains(AbilityIndex[id]) && AbilityIndex[id].IsActivateableCounter())
            {
                AbilityIndex[id].Setup(true);
            }
        }

        public bool ExecutingChain = false;

        public void ResolveChain()
        {
            ExecutingChain = true;

            ChainStep();
        }

        public void ChainStep()
        {
            if (CardChain.Count == 0)
            {
                ExecutingChain = false;
                NextStep();
                return;
            }
            CardChain.Pop().Resolve();
        }

        public void StartLongRangeBattle(Bakugan attacker, params Bakugan[] targets)
        {
            if (LongRangeBattleGoing) return;
            Attacker = attacker;
            Targets = targets;
            LongRangeBattleGoing = true;
        }
    }
}
