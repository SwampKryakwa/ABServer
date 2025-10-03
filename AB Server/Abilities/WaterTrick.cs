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
        gateCopy.TransformFrom((CondTargetSelectors[1] as GateSelector)!.SelectedGate, true, Owner.Id);
        Game.Field[gateCopy.Position.X, gateCopy.Position.Y] = gateCopy;
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Aqua) && user.Owner.GateHand.Count != 0 && Game.GateIndex.Any(x => x.OnField && x.Owner.TeamId != user.Owner.TeamId && !x.IsOpen);

    [ModuleInitializer]
    internal static void Init() => Register(47, CardKind.NormalAbility, (cID, owner) => new WaterTrick(cID, owner, 47));
}
