using AB_Server.Abilities;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;

namespace AB_Server.Gates;

abstract class GateCard(int cID, Player owner) : IBakuganContainer, IActive, IChainable
{
    public Bakugan User { get; set; }

    static Func<int, Player, GateCard>[] GateCtrs =
    [
        //Vol. 1 Gate Cards
        (x, y) => new LevelDown(x, y),
        (x, y) => new Peacemaker(x, y),
        (x, y) => new Warlock(x, y),
        (x, y) => new CheeringBattle(x, y),
        (x, y) => new Transform(x, y),

        //Vol. 2 Gate Cards
        (x, y) => new JokersWild(x, y),
        (x, y) => new PositiveDelta(x, y),
        (x, y) => new Aquamerge(x, y),
        (x, y) => new GrandSpirit(x, y),
        (x, y) => new Supernova(x, y),
        (x, y) => new Reloaded(x, y),
        (x, y) => new AdditionalTask(x, y),
        (x, y) => new QuicksandFreeze(x, y),
        (x, y) => new NegativeDelta(x, y),

        //Vol. 3 Gate Cards
        (x, y) => new ResonanceCircuit(x, y),
        (x, y) => new Shockwave(x, y),
        (x, y) => new DarkInvitation(x, y),
        (x, y) => new PowerSpike(x, y),
        (x, y) => new MindGhost(x, y),
        (x, y) => new Anastasis(x, y),

        //Vol. 3 EX Gate Cards
        (x, y) => new DirectOpposition(x, y),
        (x, y) => new WindForcement(x, y),
        (x, y) => new EnergyMerge(x, y),
        (x, y) => new DetonationZone(x, y),
    ];

    public static GateCard CreateCard(Player owner, int cID, int type)
    {
        return GateCtrs[type].Invoke(cID, owner);
    }

    public List<Bakugan[]> EnterOrder = new();

    public Game Game = owner.Game;

    public int CardId { get; set; } = cID;

    public List<Bakugan> Bakugans { get; set; } = new();
    public Player Owner { get; set; } = owner;
    public (byte X, byte Y) Position = (255, 255);
    public bool AllowAnyPlayers = false;

    public bool MarkAsIfOwnerBattling = false;
    public bool BattleStarted = false;
    public bool IsBattleGoing { get => Freezing.Count == 0 && (Bakugans.Select(x => x.Owner.TeamId).Distinct().Count() > 1 || (Bakugans.Count >= 2 && Bakugans.Any(x => x.Frenzied))); }
    public bool BattleStarting { get => !BattleStarted && IsBattleGoing; }
    public bool BattleDeclaredOver = false;
    public bool BattleOver = false;
    public bool BattleEnding { get => BattleDeclaredOver && !BattleOver; }
    public bool IsFrozen { get => Freezing.Count != 0; }
    public bool AllowsThrows { get => ThrowBlocking.Count != 0; }
    public List<object> Freezing = new();
    public List<object> OpenBlocking = new();
    public List<object> ThrowBlocking = new();
    public List<object> MovingInEffectBlocking = new();
    public List<object> MovingAwayEffectBlocking = new();
    public bool OnField { get; set; } = false;
    public bool IsOpen { get; set; } = false;
    public bool Negated = false;

    protected int currentTarget;

    protected Selector[] CondTargetSelectors = [];
    protected Selector[] ResTargetSelectors = [];


    public void Freeze(object frozer)
    {
        Freezing.Add(frozer);

        Console.WriteLine(GetType().ToString() + " frozen");

        Console.WriteLine("Battles going: " + Game.IsBattleGoing.ToString());
        BattleStarted = false;
    }

    public void TryUnfreeze(object frozer)
    {
        Freezing.Remove(frozer);
        if (Freezing.Count == 0)
            Console.WriteLine(GetType().ToString() + " unfrozen");
        else
            Console.WriteLine(GetType().ToString() + " still frozen");
        BattleStarted = false;
    }

    protected readonly List<Bakugan> bakugansDefeatedThisBattle = [];
    public virtual void DetermineWinner()
    {
        bakugansDefeatedThisBattle.Clear();
        BattleOver = true;
        BattleStarted = false;

        foreach (Bakugan b in Bakugans)
        {
            b.JustEndedBattle = true;
        }

        List<List<Bakugan>> sides = [];

        for (int i = 0; i < Game.TeamCount; i++)
            sides.Add([.. Bakugans.Where(x => !x.Frenzied && x.Owner.TeamId == i)]);

        foreach (Bakugan bakugan in Bakugans.Where(x => x.Frenzied))
            sides.Add([bakugan]);

        BattleOver = true;

        if (sides.Count < 2) return;

        int[] teamTotals = new int[sides.Count];
        for (int i = 0; i < sides.Count; i++) teamTotals[i] = sides[i].Sum(x => x.Power);

        int winnerPower = teamTotals.Max();
        List<int> sidesToDefeat = [];
        for (int i = 0; i < sides.Count; i++)
            if (teamTotals[i] < winnerPower) sides[i].ForEach(x =>
            {
                x.JustEndedBattle = false;
                bakugansDefeatedThisBattle.Add(x);
                x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
            });

        List<List<Bakugan>> survivingSides = [.. sides.Where(x => x.Any(y => y.Position == this))];
        if (survivingSides.Count > 1)
        {
            Bakugan randomFirstBakugan = EnterOrder[0][new Random().Next(EnterOrder[0].Length)];
            for (int i = 0; i < survivingSides.Count; i++)
                if (!survivingSides[i].Contains(randomFirstBakugan))
                    survivingSides[i].ForEach(x =>
                    {
                        x.JustEndedBattle = false;
                        bakugansDefeatedThisBattle.Add(x);
                        x.MoveFromFieldToDrop(EnterOrder, MoveSource.Game);
                    });
        }

        Game.BattlesToEnd.Add(this);
    }

    public virtual void Dispose()
    {
        BattleDeclaredOver = false;
        if (!IsBattleGoing)
        {
            foreach (Bakugan b in new List<Bakugan>(Bakugans))
            {
                b.JustEndedBattle = false;
                b.MoveFromFieldToHand(EnterOrder);
            }

            IsOpen = false;
            OnField = false;
            Owner.GateDrop.Add(this);

            Game.Field[Position.X, Position.Y] = null;

            Game.ThrowEvent(EventBuilder.RemoveGate(this));
            Game.ThrowEvent(EventBuilder.SendGateToDrop(this));
        }
        else Game.NextStep();
    }

    public virtual void ToDrop()
    {
        IsOpen = false;
        OnField = false;
        Owner.GateDrop.Add(this);

        Game.Field[Position.X, Position.Y] = null;

        Game.ThrowEvent(EventBuilder.RemoveGate(this));
        Game.ThrowEvent(EventBuilder.SendGateToDrop(this));
    }

    public void Set(byte posX, byte posY)
    {
        IsOpen = false;
        Game.Field[posX, posY] = this;
        OnField = true;
        Owner.GateHand.Remove(this);
        Position = (posX, posY);
        Game.ThrowEvent(EventBuilder.GateSet(this, false), Owner.PlayerId);
        Game.ThrowEvent(Owner.PlayerId, EventBuilder.GateSet(this, true));
        Game.GateSetList.Add(this);
        Game.OnGateAdded?.Invoke(this);
    }

    public void RemoveFromHand()
    {
        if (Owner.GateHand.Contains(this))
            Owner.GateHand.Remove(this);
        Game.ThrowEvent(new()
        {
            ["Type"] = "GateRemovedFromHand",
            ["CardType"] = TypeId,
            ["CID"] = CardId,
            ["Owner"] = Owner.PlayerId
        });
    }

    public static void MultiSet(Game game, GateCard[] gateCards, (byte posX, byte posY)[] positions, byte[] setBy)
    {
        for (int i = 0; i < gateCards.Length; i++)
        {
            GateCard card = gateCards[i];
            card.IsOpen = false;
            card.OnField = true;
            card.Owner.GateHand.Remove(card);
            game.Field[positions[i].posX, positions[i].posY] = card;
            card.Position = positions[i];
            game.GateSetList.Add(card);
        }

        var settables = gateCards.Zip(setBy, (first, second) => (first, second)).ToArray();
        for (byte i = 0; i < game.PlayerCount; i++)
            game.ThrowEvent(i, EventBuilder.MultiGateSet(settables, i));

        for (int i = 0; i < gateCards.Length; i++)
            game.OnGateAdded?.Invoke(gateCards[i]);
    }

    public virtual void Retract()
    {
        Game.ThrowEvent(EventBuilder.GateRetracted(this, false), Owner.PlayerId);
        Game.ThrowEvent(Owner.PlayerId, EventBuilder.GateRetracted(this, true));
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "GateAddedToHand",
            ["Owner"] = Owner.PlayerId,
            ["Kind"] = (int)Kind,
            ["CardType"] = TypeId,
            ["CID"] = CardId
        });
        OnField = false;
        (byte posX, byte posY) = Position;
        Game.Field[posX, posY] = null;
        Owner.GateHand.Add(this);
        Position = (255, 255);
        Game.GateSetList.Remove(this);
        Game.OnGateRemoved?.Invoke(this);
    }

    public virtual void Open()
    {
        SendCondTargetForSelection();
    }

    protected void SendCondTargetForSelection()
    {
        if (CondTargetSelectors.Length <= currentTarget)
        {
            Game.OnGateOpen?.Invoke(this);
            IsOpen = true;
            Game.ActiveZone.Add(this);
            Game.CardChain.Push(this);
            EffectId = Game.NextEffectId++;
            Game.ThrowEvent(EventBuilder.GateOpen(this));
            Game.CheckChain(Owner, this);
        }
        else if (CondTargetSelectors[currentTarget].Condition())
        {
            var currentSelector = CondTargetSelectors[currentTarget];
            switch (currentSelector)
            {
                case BakuganSelector bakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "B" => EventBuilder.AnyBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BH" => EventBuilder.HandBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BF" => EventBuilder.FieldBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BG" => EventBuilder.DropBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case GateSelector gateSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "G" => throw new NotImplementedException(),
                            "GF" => EventBuilder.FieldGateSelection(currentSelector.Message, TypeId, (int)Kind, Game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GH" => EventBuilder.HandGateSelection(currentSelector.Message, TypeId, (int)Kind, Game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case AbilitySelector abilitySelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "A" => EventBuilder.AbilitySelection(currentSelector.Message, Game.AbilityIndex.Where(abilitySelector.TargetValidator)),
                            "AF" => throw new NotImplementedException(),
                            "AH" => throw new NotImplementedException(),
                            "AG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case ActiveSelector activeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                    break;

                case OptionSelector optionSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                    break;

                case AttributeSelector attributeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, Enum.GetValues<Attribute>())
                        ));
                    break;

                case PlayerSelector playerSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.PlayerSelectionEvent(currentSelector.Message, [.. Game.Players.Where(playerSelector.TargetValidator)])
                        ));
                    break;

                case YesNoSelector yesNoSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                    break;

                case GateSlotSelector slotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.FieldSlotSelection(currentSelector.Message, TypeId, (int)Kind)
                        ));
                    break;

                case MultiBakuganSelector multiBakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MB" => EventBuilder.AnyMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBH" => EventBuilder.HandMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBF" => EventBuilder.FieldMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBG" => EventBuilder.DropMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case MultiGateSelector multiGateSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MGF" => EventBuilder.FieldMultiGateSelection(currentSelector.Message, TypeId, (int)Kind, multiGateSelector.MinNumber, multiGateSelector.MaxNumber, Game.GateIndex.Where(multiGateSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case MultiGateSlotSelector multiSlotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.MultiFieldSlotSelection(currentSelector.Message, TypeId, (int)Kind, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                        ));
                    break;

                case TypeSelector typeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(true && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, [.. typeSelector.SelectableKinds.Select(x => (int)x)])));
                    break;

                default:
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
            }
            Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).PlayerId] = AcceptCondTarget;
        }
        else
        {
            currentTarget++;
            SendCondTargetForSelection();
        }
    }

    void AcceptCondTarget()
    {
        var currentSelector = CondTargetSelectors[currentTarget];
        switch (currentSelector)
        {
            case BakuganSelector bakuganSelector:
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["bakugan"]];
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["gate"]];
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = Game.AbilityIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["ability"]];
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["active"]);
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["answer"];
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["option"];
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)(int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["attribute"];
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = Game.Players[(int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["player"]];
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["posX"], (int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["posY"]);
                break;

            case MultiBakuganSelector multiBakuganSelector:
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => Game.BakuganIndex[(int)x])];
                break;

            case MultiGateSelector multiGateSelector:
                JArray gateIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["gates"];
                multiGateSelector.SelectedGates = [.. gateIds.Select(x => Game.GateIndex[(int)x])];
                break;

            case MultiGateSlotSelector multiSlotSelector:
                JArray slots = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["slots"];
                multiSlotSelector.SelectedSlots = [.. slots.Select(x => ((int)(x as JArray)![0], (int)(x as JArray)![1]))];
                break;

            case TypeSelector typeSelector:
                int cardId = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["type"];
                int cardKind = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["kind"];
                typeSelector.SelectedKind = cardKind;
                typeSelector.SelectedType = cardId;
                break;

            default:
                throw new NotImplementedException();
        }
        currentTarget++;
        SendCondTargetForSelection();
    }

    public virtual void Resolve()
    {
        currentTarget = 0;
        SendResTargetForSelection();
    }

    protected void SendResTargetForSelection()
    {
        if (currentTarget == ResTargetSelectors.Length)
        {
            Resolution();
            return;
        }
        while (!ResTargetSelectors[currentTarget].HasValidTargets(Game))
        {
            currentTarget++;
            if (currentTarget == ResTargetSelectors.Length) break;
        }
        if (currentTarget == ResTargetSelectors.Length)
        {
            Resolution();
            return;
        }
        if (ResTargetSelectors[currentTarget].Condition())
        {
            var currentSelector = ResTargetSelectors[currentTarget];
            switch (currentSelector)
            {
                case BakuganSelector bakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "B" => EventBuilder.AnyBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BH" => EventBuilder.HandBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BF" => EventBuilder.FieldBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            "BG" => EventBuilder.DropBakuganSelection(currentSelector.Message, TypeId, (int)Kind, Game.BakuganIndex.Where(bakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case GateSelector gateSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "G" => throw new NotImplementedException(),
                            "GF" => EventBuilder.FieldGateSelection(currentSelector.Message, TypeId, (int)Kind, Game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GH" => EventBuilder.HandGateSelection(currentSelector.Message, TypeId, (int)Kind, Game.GateIndex.Where(gateSelector.TargetValidator)),
                            "GG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case AbilitySelector abilitySelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "A" => EventBuilder.AbilitySelection(currentSelector.Message, Game.AbilityIndex.Where(abilitySelector.TargetValidator)),
                            "AF" => throw new NotImplementedException(),
                            "AH" => throw new NotImplementedException(),
                            "AG" => throw new NotImplementedException(),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case ActiveSelector activeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                    break;

                case OptionSelector optionSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                    break;

                case AttributeSelector attributeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, Enum.GetValues<Attribute>())
                        ));
                    break;

                case PlayerSelector playerSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.PlayerSelectionEvent(currentSelector.Message, [.. Game.Players.Where(playerSelector.TargetValidator)])
                        ));
                    break;

                case YesNoSelector yesNoSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                    break;

                case GateSlotSelector slotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.FieldSlotSelection(currentSelector.Message, TypeId, (int)Kind)
                        ));
                    break;

                case MultiBakuganSelector multiBakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MB" => EventBuilder.AnyMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBH" => EventBuilder.HandMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBF" => EventBuilder.FieldMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            "MBG" => EventBuilder.DropMultiBakuganSelection(currentSelector.Message, TypeId, (int)Kind, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber, Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case MultiGateSlotSelector multiSlotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.MultiFieldSlotSelection(currentSelector.Message, TypeId, (int)Kind, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                        ));
                    break;

                case TypeSelector typeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).PlayerId, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, [.. typeSelector.SelectableKinds.Select(x => (int)x)])));
                    break;

                default:
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
            }
            Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).PlayerId] = AcceptResTarget;
        }
        else
        {
            currentTarget++;
            SendResTargetForSelection();
        }
    }

    void AcceptResTarget()
    {
        var currentSelector = ResTargetSelectors[currentTarget];
        switch (currentSelector)
        {
            case BakuganSelector bakuganSelector:
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["bakugan"]];
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["gate"]];
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = Game.AbilityIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["ability"]];
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["active"]);
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["answer"];
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["option"];
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)(int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["attribute"];
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = Game.Players[(int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["player"]];
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["posX"], (int)Game.PlayerAnswers[Owner.PlayerId]!["array"][0]["posY"]);
                break;

            case MultiBakuganSelector multiBakuganSelector:
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => Game.BakuganIndex[(int)x])];
                break;

            case MultiGateSelector multiGateSelector:
                JArray gateIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["gates"];
                multiGateSelector.SelectedGates = [.. gateIds.Select(x => Game.GateIndex[(int)x])];
                break;

            case MultiGateSlotSelector multiSlotSelector:
                JArray slots = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["slots"];
                multiSlotSelector.SelectedSlots = [.. slots.Select(x => ((int)(x as JArray)![0], (int)(x as JArray)![1]))];
                break;

            case TypeSelector typeSelector:
                int cardId = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["type"];
                int cardKind = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).PlayerId]!["array"][0]["kind"];
                typeSelector.SelectedKind = cardKind;
                typeSelector.SelectedType = cardId;
                break;

            default:
                throw new NotImplementedException();
        }
        currentTarget++;
        SendResTargetForSelection();
    }

    protected void Resolution()
    {
        if (!Negated)
            TriggerEffect();

        Game.ChainStep();
    }

    public virtual void TriggerEffect()
    { }

    public virtual bool IsOpenable() =>
        Game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !Negated && OnField && (MarkAsIfOwnerBattling || IsBattleGoing || (Game.Targets is not null && Bakugans.Any(x => Game.Targets.Contains(x)))) && !IsOpen;

    public virtual int TypeId =>
        throw new NotImplementedException();

    public int EffectId { get; set; }

    public virtual CardKind Kind { get; } = CardKind.CommandGate;

    public bool IsAdjacent(GateCard card)
    {
        return AreAdjacent(this, card);
    }

    public static bool AreAdjacent(GateCard card1, GateCard card2)
    {
        if (!card1.OnField || !card2.OnField) return false;
        int DX = Math.Abs(card1.Position.X - card2.Position.X);
        int DY = Math.Abs(card1.Position.Y - card2.Position.Y);
        return (DX + DY) == 1;
    }

    public bool IsAdjacentVertically(GateCard card)
    {
        if (!OnField || !card.OnField) return false;
        return (card.Position.Y - Position.Y) == 0 && Math.Abs(card.Position.X - Position.X) == 1;
    }

    public bool IsAdjacentHorizontally(GateCard card)
    {
        if (!OnField || !card.OnField) return false;
        return (card.Position.X - Position.X) == 0 && Math.Abs(card.Position.Y - Position.Y) == 1;
    }

    public bool IsDiagonal(GateCard otherCard)
    {
        return Math.Abs(otherCard.Position.X - Position.X) == 1 && Math.Abs(otherCard.Position.Y - Position.Y) == 1;
    }

    public virtual void Negate(bool asCounter = false)
    {
        Negated = true;
        IsOpen = false;

        Game.ThrowEvent(EventBuilder.GateNegated(this));
    }

    public void TransformFrom(GateCard source, bool conceded, params int[] revealTo)
    {
        Bakugans = source.Bakugans;
        EnterOrder = source.EnterOrder;
        Freezing = source.Freezing;
        OnField = source.OnField;
        Position = source.Position;
        BattleStarted = source.BattleStarted;
        BattleDeclaredOver = source.BattleDeclaredOver;
        BattleOver = source.BattleOver;
        ThrowBlocking = source.ThrowBlocking;
        MovingInEffectBlocking = source.MovingInEffectBlocking;
        MovingAwayEffectBlocking = source.MovingAwayEffectBlocking;

        if (!conceded)
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "GateTransformed",
                ["FromId"] = source.CardId,
                ["FromPosX"] = source.Position.X,
                ["FromPosY"] = source.Position.Y,
                ["ToId"] = CardId,
                ["ToKind"] = (int)Kind,
                ["ToType"] = TypeId,
                ["ToOwner"] = Owner.PlayerId
            });
        else
        {
            foreach (var player in Game.Players)
                Game.ThrowEvent(player.PlayerId, new JObject
                {
                    ["Type"] = "GateTransformed",
                    ["FromId"] = source.CardId,
                    ["FromPosX"] = source.Position.X,
                    ["FromPosY"] = source.Position.Y,
                    ["ToId"] = CardId,
                    ["ToKind"] = revealTo.Contains(player.PlayerId) ? (int)Kind : -1,
                    ["ToType"] = revealTo.Contains(player.PlayerId) ? TypeId : -2,
                    ["ToOwner"] = Owner.PlayerId
                });
        }
        source.OnField = false;
    }

    public bool IsBetween(GateCard card1, GateCard card2)
    {
        if (!OnField || !card1.OnField || !card2.OnField)
            return false;

        var mx = Position.X;
        var my = Position.Y;
        var x1 = card1.Position.X;
        var y1 = card1.Position.Y;
        var x2 = card2.Position.X;
        var y2 = card2.Position.Y;

        // Horizontal alignment
        if (y1 == y2 && my == y1)
        {
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            return mx > minX && mx < maxX;
        }
        // Vertical alignment
        if (x1 == x2 && mx == x1)
        {
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);
            return my > minY && my < maxY;
        }
        return false;
    }
}
