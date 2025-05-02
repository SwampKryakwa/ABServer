using AB_Server.Abilities;
using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server
{
    internal class NewGame
    {
        //Static data
        static readonly (byte X, byte Y)[] FirstCardPositions =
        [
            (0, 1),
            (1, 1),
            (0, 2),
            (1, 2)
        ];

        //Player data
        public Dictionary<long, int> UidToPid = [];
        public byte PlayerCount;
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
        public GateCard?[,] Field = new GateCard[2, 3];
        public List<GateCard> GateSetList = [];
        public List<IActive> ActiveZone = [];
        public List<IChainable> CardChain = [];

        //Game state
        bool Over = false;
        public int turnNumber = 0;
        public byte TurnPlayer;
        public byte ActivePlayer;
        public bool isBattleGoing { get => GateIndex.Any(x => x.OnField && x.ActiveBattle); }
        public ActivationWindow CurrentWindow = ActivationWindow.Normal;
        public readonly List<GateCard> AutoGatesToOpen = [];
        readonly List<Player> playersPassed = [];

        //Communication with the players
        public dynamic?[] PlayerAnswers;
        public Action[] OnAnswer;

        //Game flow
        public Action NextStep;

        //Other data
        byte playersCreated = 0;
        int NextEffectId = 0;
        bool doNotMakeStep = false;

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

        public NewGame(byte playerCount)
        {
            PlayerCount = playerCount;
            Players = new Player[playerCount];
            NewEvents = new List<JObject>[playerCount];
            PlayerAnswers = new JObject[playerCount];
            OnAnswer = new Action[playerCount];
            for (byte i = 0; i < playerCount; i++)
                NewEvents[i] = [];
        }

        public int CreatePlayer(string userName, byte team, long uuid)
        {
            Players[playersCreated] = new Player(playersCreated, team, userName);
            UidToPid.Add(uuid, playersCreated);
            return playersCreated++;
        }

        public void RegisterPlayer(byte playerId, JObject deck, byte ava)
        {
            Players[playerId].Avatar = ava;
            Players[playerId].ProvideDeck(deck);
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
                    ["Gates"] = new JArray(player.GateHand.Select(g => new JObject
                    {
                        ["CID"] = g.CardId,
                        ["Type"] = g.TypeId
                    }))
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
                };
            }
        }

        public void StartTurn()
        {
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
                ThrowEvent(new()
                {
                    ["Type"] = "PhaseChange",
                    ["Phase"] = "Main"
                });
                CheckForBattles();
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
            if ((bool)PlayerAnswers[player]["array"][0]["answer"])
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
            int id = (int)PlayerAnswers[player.Id]["array"][0]["ability"];
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
                    ["GateType"] = -1,
                    ["CID"] = gate.CardId
                });
            }
        }

        bool anyBattlesStarted;
        void CheckForBattles()
        {
            anyBattlesStarted = false;
            foreach (var gate in GateIndex.Where(x => x.OnField && x.IsBattleGoing && !x.BattleStarted))
            {
                gate.CheckAutoBattleStart();
                gate.BattleStarted = true;
                anyBattlesStarted = true;
            }

            NextStep = OpenStartBattleGates;
            OpenStartBattleGates();
        }

        void OpenStartBattleGates()
        {
            if (AutoGatesToOpen.Count == 0)
            {
                NextStep = ThrowMoveStart;
                if (anyBattlesStarted)
                    SuggestWindow(ActivationWindow.BattleStart, TurnPlayer, TurnPlayer);
                else
                    ThrowMoveStart();
            }
            else
            {
                while (!AutoGatesToOpen.Any(x => x.Owner.Id == ActivePlayer))
                {
                    ActivePlayer++;
                    if (ActivePlayer > PlayerCount) ActivePlayer = 0;
                }
                if (ActivePlayer > PlayerCount) ActivePlayer = 0;

                NewEvents[ActivePlayer].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldGateSelection("INFO_OPENSTARTBATTLE", 0, 0, AutoGatesToOpen.Where(x => x.Owner.Id == ActivePlayer))
                ));
                OnAnswer[ActivePlayer] = () =>
                {
                    AutoGatesToOpen.Remove(GateIndex[(int)PlayerAnswers[ActivePlayer]["array"][0]["gate"]]);
                    GateIndex[(int)PlayerAnswers[ActivePlayer]["array"][0]["gate"]].Open();
                    ActivePlayer++;
                };
            }
        }

        void ThrowMoveStart()
        {
            ThrowEvent(new JObject { ["Type"] = "PlayerTurnStart", ["PID"] = ActivePlayer });
        }

        public void GameStep(JObject selection)
        {
            string moveType = selection["Type"].ToString();

            bool DontThrowTurnStartEvent = false;
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
                        //EndTurn();
                        DontThrowTurnStartEvent = true;
                    }
                    break;
                case "draw":
                    var toSuggestDraw = Players.First(x => x.Id != ActivePlayer).Id;
                    NewEvents[toSuggestDraw].Add(EventBuilder.SelectionBundler(EventBuilder.BoolSelectionEvent("INFO_SUGGESTDRAW")));
                    OnAnswer[toSuggestDraw] = () =>
                    {
                        bool answer = (bool)PlayerAnswers[toSuggestDraw]["array"][0]["answer"];
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
    }
}
