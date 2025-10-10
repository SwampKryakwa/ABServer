using System.Runtime.CompilerServices;
using AB_Server.Gates;
namespace AB_Server.Abilities;

internal class SaurusGlow(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
        new SaurusGlowMarker(User, TypeId, IsCopy).Activate();

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Saurus && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(18, CardKind.NormalAbility, (cID, owner) => new SaurusGlow(cID, owner, 18));
}

internal class SaurusGlowMarker : IActive
{
    public int TypeId { get; }
    public int EffectId { get; set; }
    public CardKind Kind { get; } = CardKind.NormalAbility;
    public Bakugan User { get; set; }
    Game game { get => User.Game; }

    public Player Owner { get; set; }
    bool IsCopy;

    public SaurusGlowMarker(Bakugan user, int typeID, bool IsCopy)
    {
        User = user;
        this.IsCopy = IsCopy; Owner = user.Owner;
        TypeId = typeID;
        EffectId = game.NextEffectId++;
    }

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, IsCopy));

        game.OnBakugansFromHandsToField += OnBakuganAdded;
        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnBakuganAdded((Bakugan bakugan, GateCard destination)[] additions)
    {
        foreach ((Bakugan bakugan, GateCard _) in additions)
            if (bakugan.BasePower > User.BasePower)
                User.Boost(new Boost(50), this);
    }

    public void Negate(bool asCounter) => StopEffect();

    private void OnUserDestroyed()
    {
        StopEffect();
    }

    void StopEffect()
    {
        game.ActiveZone.Remove(this);

        game.OnBakugansFromHandsToField -= OnBakuganAdded;
        User.OnDestroyed -= OnUserDestroyed;

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
    }
}
