using AB_Server.Gates;
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
        Owner.AbilityBlockers.Count == 0 && Owner.RedAbilityBlockers.Count == 0 && Owner.BakuganOwned.Any(IsActivateableByBakugan);
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
            EventBuilder.AnyBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(UserValidator))
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
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, Enum.GetValues<Attribute>())
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, [.. typeSelector.SelectableKinds.Select(x => (int)x)])));
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
                        EventBuilder.AttributeSelectionEvent(currentSelector.Message, Enum.GetValues<Attribute>())
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
                    Game.ThrowEvent(Game.Players.First(currentSelector.ForPlayer).Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.CardTypeSelection(typeSelector.Message, [.. typeSelector.SelectableKinds.Select(x => (int)x)])));
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
                selector.SelectedBakugan = Bakugan.GetDummy();
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

    // Add these methods to your AbilityCard class

    /// <summary>
    /// Recursively checks if a valid target chain exists for all CondTargetSelectors starting from selectorIndex.
    /// Restores selector states to their original values after checking.
    /// </summary>
    protected bool HasValidTargetChain(int selectorIndex = 0)
    {
        // Base case: we've satisfied all selectors
        if (selectorIndex >= CondTargetSelectors.Length)
            return true;

        var selector = CondTargetSelectors[selectorIndex];

        // Check if this selector's condition is met
        if (!selector.Condition())
        {
            // Skip this selector and move to next
            return HasValidTargetChain(selectorIndex + 1);
        }

        // Check if this selector has any valid targets at all
        if (!selector.HasValidTargets(Game))
            return false;

        // Save the current state of this selector
        var previousState = GetSelectorState(selector);

        var validTargets = EnumeratePossibleTargets(selector);

        foreach (var target in validTargets)
        {
            SetSelectorTarget(selector, target);

            // Check if the next selector (if any) has valid targets with this selection
            bool nextSelectorValid = selectorIndex + 1 >= CondTargetSelectors.Length ||
                                     !CondTargetSelectors[selectorIndex + 1].Condition() ||
                                     CondTargetSelectors[selectorIndex + 1].HasValidTargets(Game);

            if (nextSelectorValid)
            {
                // Recurse to next selector
                if (HasValidTargetChain(selectorIndex + 1))
                {
                    // Restore the previous state before returning
                    RestoreSelectorState(selector, previousState);
                    return true;
                }
            }

            // Restore to previous state before trying next target
            RestoreSelectorState(selector, previousState);
        }

        return false;
    }

    /// <summary>
    /// Gets the current state of a selector's selected target.
    /// </summary>
    private object GetSelectorState(Selector selector)
    {
        switch (selector)
        {
            case BakuganSelector bakuganSelector:
                return bakuganSelector.SelectedBakugan;

            case GateSelector gateSelector:
                return gateSelector.SelectedGate;

            case AbilitySelector abilitySelector:
                return abilitySelector.SelectedAbility;

            case ActiveSelector activeSelector:
                return activeSelector.SelectedActive;

            case AttributeSelector attributeSelector:
                return attributeSelector.SelectedAttribute;

            case PlayerSelector playerSelector:
                return playerSelector.SelectedPlayer;

            case GateSlotSelector slotSelector:
                return slotSelector.SelectedSlot;

            case YesNoSelector yesNoSelector:
                return yesNoSelector.IsYes;

            case OptionSelector optionSelector:
                return optionSelector.SelectedOption;

            case TypeSelector typeSelector:
                return (typeSelector.SelectedKind, typeSelector.SelectedType);

            case MultiBakuganSelector multiBakuganSelector:
                return multiBakuganSelector.SelectedBakugans;

            case MultiGateSelector multiGateSelector:
                return multiGateSelector.SelectedGates;

            case MultiGateSlotSelector multiSlotSelector:
                return multiSlotSelector.SelectedSlots;

            default:
                throw new NotImplementedException($"Selector type {selector.GetType().Name} not implemented in GetSelectorState.");
        }
    }

    /// <summary>
    /// Restores a selector's selected target to a previously saved state.
    /// </summary>
    private void RestoreSelectorState(Selector selector, object previousState)
    {
        switch (selector)
        {
            case BakuganSelector bakuganSelector:
                bakuganSelector.SelectedBakugan = (Bakugan)previousState;
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = (GateCard)previousState;
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = (AbilityCard)previousState;
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = (IActive)previousState;
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)previousState;
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = (Player)previousState;
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int, int))previousState;
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)previousState;
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)previousState;
                break;

            case TypeSelector typeSelector:
                var (kind, type) = ((int, int))previousState;
                typeSelector.SelectedKind = kind;
                typeSelector.SelectedType = type;
                break;

            case MultiBakuganSelector multiBakuganSelector:
                multiBakuganSelector.SelectedBakugans = (Bakugan[])previousState;
                break;

            case MultiGateSelector multiGateSelector:
                multiGateSelector.SelectedGates = (GateCard[])previousState;
                break;

            case MultiGateSlotSelector multiSlotSelector:
                multiSlotSelector.SelectedSlots = ((int, int)[])previousState;
                break;

            default:
                throw new NotImplementedException($"Selector type {selector.GetType().Name} not implemented in RestoreSelectorState.");
        }
    }

    /// <summary>
    /// Enumerates all possible valid targets for a given selector in the current context.
    /// </summary>
    private IEnumerable<object> EnumeratePossibleTargets(Selector selector)
    {
        switch (selector)
        {
            case BakuganSelector bakuganSelector:
                return Game.BakuganIndex.Where(bakuganSelector.TargetValidator).Cast<object>();

            case GateSelector gateSelector:
                return Game.GateIndex.Where(gateSelector.TargetValidator).Cast<object>();

            case AbilitySelector abilitySelector:
                return Game.AbilityIndex.Where(abilitySelector.TargetValidator).Cast<object>();

            case ActiveSelector activeSelector:
                return Game.ActiveZone.Where(activeSelector.TargetValidator).Cast<object>();

            case AttributeSelector attributeSelector:
                return Enum.GetValues(typeof(Attribute))
                    .Cast<Attribute>()
                    .Where(attributeSelector.TargetValidator)
                    .Cast<object>();

            case PlayerSelector playerSelector:
                return Game.Players.Where(playerSelector.TargetValidator).Cast<object>();

            case GateSlotSelector slotSelector:
                // Enumerate all empty slots on the field
                var slots = new List<(int, int)>();
                for (int x = 0; x < Game.Field.GetLength(0); x++)
                {
                    for (int y = 0; y < Game.Field.GetLength(1); y++)
                    {
                        if (Game.Field[x, y] == null && slotSelector.TargetValidator(x, y))
                            slots.Add((x, y));
                    }
                }
                return slots.Cast<object>();

            case YesNoSelector yesNoSelector:
                // Yes/No selector has two possible values
                return new object[] { true, false };

            case OptionSelector optionSelector:
                // Option selector has N possible values (0 to OptionCount-1)
                return Enumerable.Range(0, optionSelector.OptionCount).Cast<object>();

            case TypeSelector typeSelector:
                // For type selector, enumerate all possible card types of the allowed kinds
                var types = new List<(int kind, int type)>();
                foreach (CardKind kind in typeSelector.SelectableKinds)
                {
                    int maxType = kind == CardKind.CorrelationAbility
                        ? CorrelationCtrs.Length
                        : AbilityCtrs.Length;

                    for (int typeId = 0; typeId < maxType; typeId++)
                    {
                        types.Add(((int)kind, typeId));
                    }
                }
                return types.Cast<object>();

            case MultiBakuganSelector multiBakuganSelector:
                // For multi-selectors, we need to generate all valid combinations
                // This is complex - we'll generate all valid subsets
                var validBakugans = Game.BakuganIndex.Where(multiBakuganSelector.TargetValidator).ToList();
                return GenerateCombinations(validBakugans, multiBakuganSelector.MinNumber, multiBakuganSelector.MaxNumber)
                    .Select(combo => combo.ToArray())
                    .Cast<object>();

            case MultiGateSelector multiGateSelector:
                var validGates = Game.GateIndex.Where(multiGateSelector.TargetValidator).ToList();
                return GenerateCombinations(validGates, multiGateSelector.MinNumber, multiGateSelector.MaxNumber)
                    .Select(combo => combo.ToArray())
                    .Cast<object>();

            case MultiGateSlotSelector multiSlotSelector:
                // Generate all empty slots
                var allSlots = new List<(int, int)>();
                for (int x = 0; x < Game.Field.GetLength(0); x++)
                {
                    for (int y = 0; y < Game.Field.GetLength(1); y++)
                    {
                        if (Game.Field[x, y] == null)
                            allSlots.Add((x, y));
                    }
                }
                // Generate all valid combinations of slots
                return GenerateCombinations(allSlots, multiSlotSelector.MinNumber, multiSlotSelector.MaxNumber)
                    .Select(combo => combo.ToArray())
                    .Cast<object>();

            default:
                throw new NotImplementedException($"Selector type {selector.GetType().Name} not implemented in EnumeratePossibleTargets.");
        }
    }

    /// <summary>
    /// Sets the selector's selected target to the given value.
    /// </summary>
    private void SetSelectorTarget(Selector selector, object target)
    {
        switch (selector)
        {
            case BakuganSelector bakuganSelector:
                bakuganSelector.SelectedBakugan = (Bakugan)target;
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = (GateCard)target;
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = (AbilityCard)target;
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = (IActive)target;
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = (Attribute)target;
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = (Player)target;
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = ((int, int))target;
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = (bool)target;
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = (int)target;
                break;

            case TypeSelector typeSelector:
                var (kind, type) = ((int, int))target;
                typeSelector.SelectedKind = kind;
                typeSelector.SelectedType = type;
                break;

            case MultiBakuganSelector multiBakuganSelector:
                multiBakuganSelector.SelectedBakugans = (Bakugan[])target;
                break;

            case MultiGateSelector multiGateSelector:
                multiGateSelector.SelectedGates = (GateCard[])target;
                break;

            case MultiGateSlotSelector multiSlotSelector:
                multiSlotSelector.SelectedSlots = ((int, int)[])target;
                break;

            default:
                throw new NotImplementedException($"Selector type {selector.GetType().Name} not implemented in SetSelectorTarget.");
        }
    }

    /// <summary>
    /// Resets the selector's selected target to null/default.
    /// </summary>
    private void ResetSelectorTarget(Selector selector)
    {
        switch (selector)
        {
            case BakuganSelector bakuganSelector:
                bakuganSelector.SelectedBakugan = null;
                break;

            case GateSelector gateSelector:
                gateSelector.SelectedGate = null;
                break;

            case AbilitySelector abilitySelector:
                abilitySelector.SelectedAbility = null;
                break;

            case ActiveSelector activeSelector:
                activeSelector.SelectedActive = null;
                break;

            case AttributeSelector attributeSelector:
                attributeSelector.SelectedAttribute = Attribute.Clear;
                break;

            case PlayerSelector playerSelector:
                playerSelector.SelectedPlayer = null;
                break;

            case GateSlotSelector slotSelector:
                slotSelector.SelectedSlot = default;
                break;

            case YesNoSelector yesNoSelector:
                yesNoSelector.IsYes = false;
                break;

            case OptionSelector optionSelector:
                optionSelector.SelectedOption = -1;
                break;

            case TypeSelector typeSelector:
                typeSelector.SelectedKind = 0;
                typeSelector.SelectedType = 0;
                break;

            case MultiBakuganSelector multiBakuganSelector:
                multiBakuganSelector.SelectedBakugans = null;
                break;

            case MultiGateSelector multiGateSelector:
                multiGateSelector.SelectedGates = null;
                break;

            case MultiGateSlotSelector multiSlotSelector:
                multiSlotSelector.SelectedSlots = null;
                break;

            default:
                throw new NotImplementedException($"Selector type {selector.GetType().Name} not implemented in ResetSelectorTarget.");
        }
    }

    /// <summary>
    /// Generates all combinations of items with size between minCount and maxCount.
    /// </summary>
    private IEnumerable<IEnumerable<T>> GenerateCombinations<T>(List<T> items, int minCount, int maxCount)
    {
        maxCount = Math.Min(maxCount, items.Count);

        for (int size = minCount; size <= maxCount; size++)
        {
            foreach (var combo in GenerateCombinationsOfSize(items, size))
            {
                yield return combo;
            }
        }
    }

    /// <summary>
    /// Generates all combinations of a specific size.
    /// </summary>
    private IEnumerable<IEnumerable<T>> GenerateCombinationsOfSize<T>(List<T> items, int size)
    {
        if (size == 0)
        {
            yield return Enumerable.Empty<T>();
            yield break;
        }

        if (size > items.Count)
            yield break;

        for (int i = 0; i <= items.Count - size; i++)
        {
            var item = items[i];
            var remainingItems = items.Skip(i + 1).ToList();

            foreach (var combo in GenerateCombinationsOfSize(remainingItems, size - 1))
            {
                yield return new[] { item }.Concat(combo);
            }
        }
    }

    /// <summary>
    /// Updated IsActivateableByBakugan that uses the target chain checking system.
    /// </summary>
    public virtual bool IsActivateableByBakugan(Bakugan user)
    {
        // Set up context
        User = user;

        // Check if a valid target chain exists
        bool hasValidChain = HasValidTargetChain(0);

        // Also check any extra conditions
        return hasValidChain && UserValidator(user) && ActivationCondition();
    }

    /// <summary>
    /// Virtual method for card-specific extra activation conditions.
    /// Override this in derived classes instead of IsActivateableByBakugan when possible.
    /// </summary>
    public virtual bool UserValidator(Bakugan user) => true;

    public virtual bool ActivationCondition() => Game.CurrentWindow == ActivationWindow.Normal;
}
