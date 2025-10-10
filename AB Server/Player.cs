using AB_Server.Abilities;
using AB_Server.Abilities.Correlations;
using AB_Server.Gates;

namespace AB_Server;

internal class BakuganDrop : IBakuganContainer
{
    public Player Player;

    public List<Bakugan> Bakugans { get; } = new();

    public BakuganDrop(Player player)
    {
        Player = player;
    }
}

internal class Player : IBakuganContainer
{
    public byte Id;
    public byte TeamId = new();
    public string DisplayName;
    public byte Avatar;
    public List<object> AbilityBlockers = new();
    public List<object> RedAbilityBlockers = new();
    public List<object> GreenAbilityBlockers = new();
    public List<object> BlueAbilityBlockers = new();
    public List<object> GateBlockers = new();

    public List<Bakugan> Bakugans { get; } = new();
    public List<AbilityCard> AbilityHand = new();
    public List<GateCard> GateHand = new();

    public BakuganDrop BakuganDrop;
    public List<AbilityCard> AbilityDrop = new();
    public List<GateCard> GateDrop = new();

    public List<Bakugan> BakuganOwned = new();

    public bool HadSetGate = false;
    public int UsedThrows = 0;
    public int AllowedThrows = 1;
    public bool HadUsedCounter = false;

    public bool Alive { get => BakuganOwned.Any(x => !x.Defeated); }

    public Game Game;

    public byte PlayerColor;

    public Player(Game game, byte id, byte sideID, string displayName)
    {
        Game = game;
        Id = id;
        TeamId = sideID;
        DisplayName = displayName;
        BakuganDrop = new(this);
    }

    public void ProvideDeck(dynamic deck)
    {
        if (deck["deck_color"] != null)
            PlayerColor = deck["deck_color"];
        else
            PlayerColor = 0;

        foreach (dynamic b in deck["bakugans"])
        {
            int type = (int)b["Type"];
            short power = (short)b["Power"];
            int attr = (int)b["Attribute"];
            int treatment = (int)b["Treatment"];

            Bakugan bak = new((BakuganType)type, power, (Attribute)attr, (Treatment)treatment, this, Game, Game.BakuganIndex.Count);
            Game.BakuganIndex.Add(bak);
            Bakugans.Add(bak);
            BakuganOwned.Add(bak);
        }

        foreach (int a in deck["abilities"])
        {
            AbilityCard abi = AbilityCard.CreateCard(this, Game.AbilityIndex.Count, a);
            AbilityHand.Add(abi);
            Game.AbilityIndex.Add(abi);
        }

        FusionAbility fusion = FusionAbility.FusionCtrs[deck["bakugans"][0]["Type"]].Invoke(Game.AbilityIndex.Count, this);
        AbilityHand.Add(fusion);
        Game.AbilityIndex.Add(fusion);

        BakuganOwned[0].IsPartner = true;

        if (deck.ContainsKey("correlation"))
        {
            var correlation = AbilityCard.CorrelationCtrs[deck["correlation"]].Invoke(Game.AbilityIndex.Count, this);
            AbilityHand.Add(correlation);
            Game.AbilityIndex.Add(correlation);
        }
        else
        {
            HashSet<Attribute> combinedAttributes = new(BakuganOwned.Select(x => x.BaseAttribute));
            if (combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Nova, Attribute.Lumina, Attribute.Aqua }) ||
                   combinedAttributes.SetEquals(new HashSet<Attribute> { Attribute.Zephyros, Attribute.Subterra, Attribute.Darkon }))
            {
                TripleNode tripleNode = new(Game.AbilityIndex.Count, this);
                Game.AbilityIndex.Add(tripleNode);
                AbilityHand.Add(tripleNode);
            }
            else if ((combinedAttributes.Contains(Attribute.Nova) && combinedAttributes.Contains(Attribute.Darkon)) ||
                   (combinedAttributes.Contains(Attribute.Subterra) && combinedAttributes.Contains(Attribute.Aqua)) ||
                   (combinedAttributes.Contains(Attribute.Lumina) && combinedAttributes.Contains(Attribute.Zephyros)))
            {
                DiagonalCorrelation diagonalCorrelation = new(Game.AbilityIndex.Count, this);
                Game.AbilityIndex.Add(diagonalCorrelation);
                AbilityHand.Add(diagonalCorrelation);
            }
            else if (combinedAttributes.Distinct().Count() == 1)
            {
                ElementalFlash elementResonance = new(Game.AbilityIndex.Count, this);
                Game.AbilityIndex.Add(elementResonance);
                AbilityHand.Add(elementResonance);
            }
            else
            {
                AdjacentCorrelation adjacentCorrelation = new(Game.AbilityIndex.Count, this);
                Game.AbilityIndex.Add(adjacentCorrelation);
                AbilityHand.Add(adjacentCorrelation);
            }
        }

        foreach (dynamic g in deck["gates"])
        {
            GateCard gate;
            gate = GateCard.CreateCard(this, Game.GateIndex.Count, (int)g["Type"]);

            GateHand.Add(gate);
            Game.GateIndex.Add(gate);
        }
    }
    public bool HasThrowableBakugan() =>
        Bakugans.Count != 0 && Game.Field.Cast<GateCard>().Any(x => x != null) && UsedThrows < AllowedThrows;

    public bool HasActivateableAbilities() =>
        AbilityHand.Any(x => x.IsActivateable());

    public bool HasActivateableFusionAbilities(Bakugan user) =>
        AbilityHand.Any(x => x.IsActivateableByBakugan(user));

    public bool HasSettableGates() =>
        GateHand.Count != 0 && !HadSetGate;

    public bool HasOpenableGates()
    {
        foreach (GateCard? gate in Game.Field)
        {
            if (gate is GateCard g)
                if (g.IsOpenable() && g.Owner == this)
                    return true;
        }
        return false;
    }

    public List<Bakugan> ThrowableBakugan() =>
        Bakugans;

    public List<GateCard> SettableGates() =>
        GateHand;

    //public List<AbilityCard> ActivateableAbilities()
    //{
    //    return AbilityHand.Where(x => x.IsActivateable()).ToList();
    //}

    public List<GateCard> OpenableGates()
    {
        List<GateCard> openableGates = new();
        foreach (GateCard? gate in Game.Field)
        {
            if (gate is GateCard g)
                if (g.IsOpenable() && g.Owner == this && !g.IsOpen)
                    openableGates.Add(g);
        }
        return openableGates;
    }

    public bool CanEndTurn()
    {
        return !Game.IsBattleGoing && (!HasThrowableBakugan() || UsedThrows > 0 || AllowedThrows == 0 || !Game.GateIndex.Any(x => x.OnField && !x.Bakugans.Any(x => x.Owner.TeamId == TeamId) && x.ThrowBlocking.Count == 0));
    }

    public bool HasBattlingBakugan() =>
        BakuganOwned.Any(x => x.OnField() && !x.Defeated && x.InBattle);
}
