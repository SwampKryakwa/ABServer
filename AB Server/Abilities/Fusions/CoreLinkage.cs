using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class CoreLinkage(int cID, Player owner) : FusionAbility(cID, owner, 9, typeof(Enforcement))
{
    public override void TriggerEffect() =>
        new CoreLinkageMarker(User, IsCopy).Activate();

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.OnField() && user.Type == BakuganType.Garrison && Game.CurrentWindow == ActivationWindow.Normal;

    [ModuleInitializer]
    internal static void Init() => Register(8, (cID, owner) => new CoreLinkage(cID, owner));
}

internal class CoreLinkageMarker(Bakugan user, bool asCopy) : IActive
{
    public int EffectId { get; set; } = user.Game.NextEffectId++;

    public int TypeId { get; } = 8;

    public CardKind Kind { get; } = CardKind.FusionAbility;

    public Bakugan User { get; set; } = user;
    public Player Owner { get; set; } = user.Owner;
    Game game = user.Game;
    Dictionary<Bakugan, Boost> currentBoosts = [];

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, asCopy));

        foreach (var bakugan in Owner.BakuganOwned.Where(x => x != User))
        {
            var boost = new Boost(50);
            bakugan.ContinuousBoost(boost, this);
            currentBoosts.Add(bakugan, boost);
        }

        User.OnDestroyed += OnUserDestroyed;
    }

    private void OnUserDestroyed()
    {
        User.OnDestroyed -= OnUserDestroyed;
        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        foreach ((var bakugan, var boost) in currentBoosts)
            bakugan.RemoveContinuousBoost(boost, this);
    }

    public void Negate(bool asCounter = false)
    {
        User.OnDestroyed -= OnUserDestroyed;
        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        foreach ((var bakugan, var boost) in currentBoosts)
            bakugan.RemoveContinuousBoost(boost, this);
    }
}
