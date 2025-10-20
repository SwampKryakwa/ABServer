using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class WaterTrick : AbilityCard
{
    public WaterTrick(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = b => b.OnField() },
            new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = b => b.OnField() && ResTargetSelectors[0] is BakuganSelector bakSelector && bakSelector.SelectedBakugan != b && bakSelector.SelectedBakugan.Position != b.Position }
        ];
    }

    public override void TriggerEffect()
    {
        var posGate1 = ((ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Position as GateCard)!;
        var posGate2 = ((ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan.Position as GateCard)!;
        (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Move(posGate2, new() { ["MoveEffect"] = "Slide" });
        (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.Move(posGate1, new() { ["MoveEffect"] = "Slide" });
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Aqua);

    [ModuleInitializer]
    internal static void Init() => Register(47, CardKind.NormalAbility, (cID, owner) => new WaterTrick(cID, owner, 47));
}
