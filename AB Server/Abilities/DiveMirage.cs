using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class DiveMirage : AbilityCard
{
    public DiveMirage(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x == Game.GateSetList.Last(x=>x.OnField) }
        ];
    }

    public override void TriggerEffect()
    {
        GenericEffects.MoveBakuganEffect(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, new JObject() { ["MoveEffect"] = "Submerge" });
        if (Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Aqua)))
            new DiveMirageMarker(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, Owner, Game, TypeId, Kind, IsCopy).Activate();
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Aqua) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(30, CardKind.NormalAbility, (cID, owner) => new DiveMirage(cID, owner, 30));
}

internal class DiveMirageMarker(Bakugan user, GateCard target, Player owner, Game game, int typeId, CardKind kind, bool isCopy) : IActive
{
    public int EffectId { get; set; } = game.NextEffectId++;

    public int TypeId { get; } = typeId;

    public CardKind Kind { get; } = kind;

    Bakugan IActive.User { get; set; } = user;
    Player IActive.Owner { get; set; } = owner;

    public void Activate()
    {
        game.ActiveZone.Add(this);

        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));
        target.OpenBlocking.Add(this);

        game.TurnEnd += StopEffect;
    }

    public void Negate(bool asCounter = false)
    {
        StopEffect();
    }

    void StopEffect()
    {
        game.ActiveZone.Remove(this);

        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        target.OpenBlocking.Remove(this);

        game.TurnEnd -= StopEffect;
    }
}
