using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Luminescence(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
        new LuminescenceEffect(User, IsCopy).Activate();

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(51, CardKind.NormalAbility, (cID, owner) => new Luminescence(cID, owner, 51));
}

internal class LuminescenceEffect(Bakugan user, bool isCopy) : IActive
{
    public int TypeId { get; } = 51;
    public int KindId { get; } = 0;
    public int EffectId { get; set; } = user.Game.NextEffectId++;
    public CardKind Kind { get; } = CardKind.NormalAbility;
    public Bakugan User { get; set; } = user;
    Game game { get => User.Game; }
    Dictionary<Bakugan, Boost> currentBoosts = [];
    List<Bakugan> targets = [];
    public Player Owner { get; set; } = user.Owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

        currentBoosts = [];
        foreach (var bak in User.Owner.BakuganOwned.Where(x => x.IsAttribute(Attribute.Lumina)))
        {
            Boost boost = new(30);
            currentBoosts.Add(bak, boost);
            targets.Add(bak);
            bak.ContinuousBoost(boost, this);
        }

        game.BakuganAttributeChanged += OnAttributeChange;
    }

    public void OnAttributeChange(Bakugan target)
    {
        if (User.IsAttribute(Attribute.Lumina) && !targets.Contains(target))
        {
            Boost boost = new(30);
            currentBoosts.Add(target, boost);
            targets.Add(target);
            target.ContinuousBoost(boost, this);
        }
        else if (!User.IsAttribute(Attribute.Lumina) && targets.Contains(target))
        {
            Boost boost = currentBoosts[target];
            boost.Active = false;
            currentBoosts.Remove(target);
            targets.Remove(target);
            target.RemoveContinuousBoost(boost, this);
        }
    }


    public void Negate(bool asCounter)
    {
        game.ActiveZone.Remove(this);

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));

        foreach (var target in targets)
        {
            Boost boost = currentBoosts[target];
            boost.Active = false;
            currentBoosts.Remove(target);
            targets.Remove(target);
            target.RemoveContinuousBoost(boost, this);
        }

        game.BakuganAttributeChanged -= OnAttributeChange;
    }
}

