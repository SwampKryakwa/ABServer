using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities;

public enum CardKind : byte
{
    NormalAbility,
    FusionAbility,
    CorrelationAbility,
    CommandGate,
    SpecialGate
}

abstract class AbilityCard(int cID, Player owner, int typeId) : IActive, IChainable
{
    public static Func<int, Player, AbilityCard>[] AbilityCtrs = Array.Empty<Func<int, Player, AbilityCard>>();
    public static Func<int, Player, AbilityCard>[] CorrelationCtrs = Array.Empty<Func<int, Player, AbilityCard>>();

    internal static void Register(int typeId, CardKind kind, Func<int, Player, AbilityCard> constructor)
    {
        if (kind == CardKind.CorrelationAbility)
        {
            if (CorrelationCtrs.Length <= typeId)
                Array.Resize(ref CorrelationCtrs, typeId + 1);
            CorrelationCtrs[typeId] = constructor;
            return;
        }

        if (AbilityCtrs.Length <= typeId)
            Array.Resize(ref AbilityCtrs, typeId + 1);
        AbilityCtrs[typeId] = constructor;
    }

    public static AbilityCard CreateCard(Player owner, int cID, int type)
    {
        if (type < 0 || type >= AbilityCtrs.Length || AbilityCtrs[type] is null)
            throw new InvalidOperationException($"Ability type id {type} not registered.");
        return AbilityCtrs[type].Invoke(cID, owner);
    }
    public bool counterNegated { get; set; } = false;
    public bool IsCopy { get; set; } = false;

    public int TypeId { get; set; } = typeId;
    public virtual CardKind Kind { get; } = CardKind.NormalAbility;

    public Game Game { get; set; } = owner.Game;
    public Player Owner { get; set; } = owner;
    public int EffectId { get; set; }

    public int CardId { get; protected set; } = cID;

    public Bakugan User { get; set; }

    public virtual bool IsActivateable() =>
        Game.BakuganIndex.Any(BakuganIsValid);
    public virtual bool BakuganIsValid(Bakugan user) =>
        Owner.AbilityBlockers.Count == 0 && Owner.RedAbilityBlockers.Count == 0 && !user.Frenzied && IsActivateableByBakugan(user) && user.Owner == Owner;
    public abstract bool IsActivateableByBakugan(Bakugan user);
    public virtual bool IsActivateableCounter() => IsActivateable();

    public static bool HasValidTargets(Bakugan user) => true;

    protected bool asCounter;

    protected int currentTarget;

    protected Selector[] CondTargetSelectors = [];
    protected Selector[] ResTargetSelectors = [];

    public virtual void Setup(bool asCounter)
    {
        this.asCounter = asCounter;
        if (!asCounter && Game.CurrentWindow == ActivationWindow.Normal)
            Game.OnCancel[Owner.Id] = Game.ThrowMoveStart;
        Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
            EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

        Game.OnAnswer[Owner.Id] = RecieveUser;
    }

    protected void RecieveUser()
    {
        currentTarget = 0;
        User = Game.BakuganIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["bakugan"]];
        SendCondTargetForSelection();
    }

    protected void SendCondTargetForSelection()
    {
        if (!asCounter && Game.CurrentWindow == ActivationWindow.Normal)
            Game.OnCancel[Owner.Id] = Game.ThrowMoveStart;
        if (CondTargetSelectors.Length <= currentTarget) Activate();
        else if (CondTargetSelectors[currentTarget].Condition())
        {
            var currentSelector = CondTargetSelectors[currentTarget];
            switch (currentSelector)
            {
                case BakuganSelector bakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                    break;

                case OptionSelector optionSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                    break;

                case AttributeSelector attributeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, [.. Enum.GetValues(typeof(Attribute)).Cast<Attribute>().Where(attributeSelector.TargetValidator)])
                        ));
                    break;

                case PlayerSelector playerSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.PlayerSelectionEvent(currentSelector.Message, [.. Game.Players.Where(playerSelector.TargetValidator)])
                        ));
                    break;

                case YesNoSelector yesNoSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                    break;

                case GateSlotSelector slotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.FieldSlotSelection(currentSelector.Message, TypeId, (int)Kind)
                        ));
                    break;

                case MultiBakuganSelector multiBakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        currentSelector.ClientType switch
                        {
                            "MGF" => EventBuilder.FieldMultiGateSelection(currentSelector.Message, TypeId, (int)Kind, multiGateSelector.MinNumber, multiGateSelector.MaxNumber, Game.GateIndex.Where(multiGateSelector.TargetValidator)),
                            _ => throw new NotImplementedException()
                        }
                        ));
                    break;

                case MultiGateSlotSelector multiSlotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.MultiFieldSlotSelection(currentSelector.Message, TypeId, (int)Kind, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                        ));
                    break;

                case TypeSelector typeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, typeSelector.SelectableKinds.Select(x => (int)x).ToArray())));
                    break;

                default:
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
            }
            Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).Id] = AcceptCondTarget;
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
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = Game.AbilityIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["ability"]];
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["attribute"];
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = Game.Players[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["player"]];
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int)Game.PlayerAnswers[Owner.Id]!["array"][0]["posX"], (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["posY"]);
                break;

            case MultiBakuganSelector multiBakuganSelector:
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => Game.BakuganIndex[(int)x])];
                break;

            case MultiGateSelector multiGateSelector:
                JArray gateIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gates"];
                multiGateSelector.SelectedGates = [.. gateIds.Select(x => Game.GateIndex[(int)x])];
                break;

            case MultiGateSlotSelector multiSlotSelector:
                JArray slots = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["slots"];
                multiSlotSelector.SelectedSlots = [.. slots.Select(x => ((int)(x as JArray)![0], (int)(x as JArray)![1]))];
                break;

            case TypeSelector typeSelector:
                int cardId = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["type"];
                int cardKind = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["kind"];
                typeSelector.SelectedKind = cardKind;
                typeSelector.SelectedType = cardId;
                break;

            default:
                throw new NotImplementedException();
        }
        currentTarget++;
        SendCondTargetForSelection();
    }

    public virtual void Activate()
    {
        EffectId = Game.NextEffectId++;
        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityAddedActiveZone",
            ["IsCopy"] = IsCopy,
            ["Id"] = EffectId,
            ["Card"] = TypeId,
            ["Kind"] = (int)Kind,
            ["User"] = User.BID,
            ["IsCounter"] = asCounter,
            ["Owner"] = Owner.Id
        });

        Game.ActiveZone.Add(this);
        Game.CardChain.Push(this);
        Game.CheckChain(Owner, this, User);
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.ActiveSelection(currentSelector.Message, TypeId, (int)Kind, Game.ActiveZone.Where(activeSelector.TargetValidator))
                        ));
                    break;

                case OptionSelector optionSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.OptionSelectionEvent(currentSelector.Message, optionSelector.OptionCount)
                        ));
                    break;

                case AttributeSelector attributeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, [.. Enum.GetValues(typeof(Attribute)).Cast<Attribute>().Where(attributeSelector.TargetValidator)])
                        ));
                    break;

                case PlayerSelector playerSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.PlayerSelectionEvent(currentSelector.Message, [.. Game.Players.Where(playerSelector.TargetValidator)])
                        ));
                    break;

                case YesNoSelector yesNoSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.BoolSelectionEvent(yesNoSelector.Message)
                        ));
                    break;

                case GateSlotSelector slotSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.FieldSlotSelection(currentSelector.Message, TypeId, (int)Kind)
                        ));
                    break;

                case MultiBakuganSelector multiBakuganSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal,
                        EventBuilder.MultiFieldSlotSelection(currentSelector.Message, TypeId, (int)Kind, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                        ));
                    break;

                case TypeSelector typeSelector:
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, typeSelector.SelectableKinds.Select(x => (int)x).ToArray())));
                    break;

                default:
                    Console.WriteLine(GetType());
                    Console.WriteLine(currentSelector.GetType());
                    throw new NotImplementedException();
            }
            Game.OnAnswer[Game.Players.First(currentSelector.ForPlayer).Id] = AcceptResTarget;
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
                bakuganSelector.SelectedBakugan = Game.BakuganIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugan"]];
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = Game.GateIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gate"]];
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = Game.AbilityIndex[(int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["ability"]];
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = Game.ActiveZone.First(x => x.EffectId == (int)Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["active"]);
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)Game.PlayerAnswers[Owner.Id]!["array"][0]["answer"];
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["option"];
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["attribute"];
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = Game.Players[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["player"]];
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int)Game.PlayerAnswers[Owner.Id]!["array"][0]["posX"], (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["posY"]);
                break;

            case MultiBakuganSelector multiBakuganSelector:
                JArray bakuganIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["bakugans"];
                multiBakuganSelector.SelectedBakugans = [.. bakuganIds.Select(x => Game.BakuganIndex[(int)x])];
                break;

            case MultiGateSelector multiGateSelector:
                JArray gateIds = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["gates"];
                multiGateSelector.SelectedGates = [.. gateIds.Select(x => Game.GateIndex[(int)x])];
                break;

            case MultiGateSlotSelector multiSlotSelector:
                JArray slots = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["slots"];
                multiSlotSelector.SelectedSlots = [.. slots.Select(x => ((int)(x as JArray)![0], (int)(x as JArray)![1]))];
                break;

            case TypeSelector typeSelector:
                int cardId = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["type"];
                int cardKind = Game.PlayerAnswers[Game.Players.First(currentSelector.ForPlayer).Id]!["array"][0]["kind"];
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
        if (!counterNegated)
            TriggerEffect();
        Dispose();
        Game.ChainStep();
    }

    public virtual void DoNotAffect(Bakugan bakugan)
    {
        if (User == bakugan)
            User = Bakugan.GetDummy();
        foreach (BakuganSelector selector in CondTargetSelectors.Where(x => x is BakuganSelector))
            if (selector.SelectedBakugan == bakugan)
                selector.SelectedBakugan = bakugan;
    }

    public virtual void Dispose()
    {
        Discard();
        if (Game.ActiveZone.Contains(this))
        {
            Game.ActiveZone.Remove(this);

            Game.ThrowEvent(new()
            {
                ["Type"] = "AbilityRemovedActiveZone",
                ["Id"] = EffectId,
                ["Card"] = TypeId,
                ["Owner"] = Owner.Id
            });
        }
        Game.ThrowEvent(EventBuilder.SendAbilityToDrop(this));
    }

    public virtual void Discard()
    {
        if (Owner.AbilityHand.Contains(this))
            Owner.AbilityHand.Remove(this);
        if (!IsCopy)
            Owner.AbilityDrop.Add(this);

        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityRemovedFromHand",
            ["CID"] = CardId,
            ["Owner"] = Owner.Id
        });
    }

    public void FromDropToHand()
    {
        if (Owner.AbilityDrop.Contains(this))
        {
            Owner.AbilityDrop.Remove(this);
            Owner.AbilityHand.Add(this);
        }

        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityAddedToHand",
            ["Kind"] = (int)Kind,
            ["CardType"] = TypeId,
            ["CID"] = CardId,
            ["Owner"] = Owner.Id
        });

        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityRemovedFromDrop",
            ["Id"] = EffectId,
            ["Owner"] = Owner.Id
        });
    }

    public virtual void Retract()
    {
        Game.ActiveZone.Remove(this);
        if (!IsCopy)
            Owner.AbilityHand.Add(this);

        Game.ThrowEvent(new()
        {
            ["Type"] = "AbilityRemovedActiveZone",
            ["Id"] = EffectId,
            ["Card"] = TypeId,
            ["Kind"] = (int)Kind,
            ["CardType"] = TypeId,
            ["Owner"] = Owner.Id
        });
    }

    public abstract void TriggerEffect();

    public virtual void Negate(bool asCounter)
    {
        if (asCounter)
            counterNegated = true;
    }
}
