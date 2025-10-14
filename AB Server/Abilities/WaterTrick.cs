using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class WaterTrick : AbilityCard
{
    public WaterTrick(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.Owner.GateHand.Contains(x) },
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.Owner != Owner && x.OnField && !x.IsOpen }
        ];
    }

    public override void TriggerEffect()
    {
        var gateCopy = GateCard.CreateCard((CondTargetSelectors[1] as GateSelector)!.SelectedGate.Owner, Game.GateIndex.Count, (CondTargetSelectors[0] as GateSelector)!.SelectedGate.TypeId);
        Game.GateIndex.Add(gateCopy);
        gateCopy.TransformFrom((CondTargetSelectors[1] as GateSelector)!.SelectedGate, true, Owner.PlayerId);
        Game.Field[gateCopy.Position.X, gateCopy.Position.Y] = gateCopy;
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(47, CardKind.NormalAbility, (cID, owner) => new WaterTrick(cID, owner, 47));
}
