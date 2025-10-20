using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class LightLine(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        // Get all Haos Bakugan on the field owned by this card's owner
        var haosBakugan = Game.BakuganIndex
            .Where(b => b.OnField() && b.Owner == Owner && b.IsAttribute(Attribute.Lumina))
            .ToArray();

        var numMyBakuganNotUserOnField = Game.BakuganIndex.Where(b => b.OnField() && b.Owner == Owner && b != User).Count();

        foreach (var bak in haosBakugan)
            bak.Boost(100 * numMyBakuganNotUserOnField, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Lumina);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Init() =>
        Register(53, CardKind.NormalAbility, (cID, owner) => new LightLine(cID, owner, 53));
}

internal class LightLineMarker(Bakugan user, bool isCopy) : IActive
{
    public int EffectId { get; set; } = user.Game.NextEffectId++;

    public int TypeId { get => 53; }

    public CardKind Kind { get => CardKind.NormalAbility; }

    public Bakugan User { get; set; } = user;
    public Player Owner { get; set; } = user.Owner;
    Game game = user.Game;
    
    public void Activate()
    {
        game.ActiveZone.Add(this);
        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

        game.OnBakugansFromDropToField += OnBakugansToField;
        game.OnBakugansFromHandsToField += OnBakugansToField;
    }

    private void OnBakugansToField((Bakugan, GateCard)[] obj)
    {
        foreach ((Bakugan bakugan, GateCard _) in obj)
            if (bakugan.Owner == Owner && Owner.BakuganOwned.Count(x=>x.OnField()) >= 2)
                foreach (var bak in Owner.BakuganOwned.Where(x => x.OnField()))
                    bak.Boost(80, this);
    }

    public void Negate(bool asCounter = false)
    {
        game.ActiveZone.Remove(this);
        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));

        game.OnBakugansFromDropToField -= OnBakugansToField;
        game.OnBakugansFromHandsToField -= OnBakugansToField;
    }
}