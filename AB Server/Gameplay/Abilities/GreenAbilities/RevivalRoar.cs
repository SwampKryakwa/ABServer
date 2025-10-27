using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class RevivalRoar : FusionAbility
{
    public RevivalRoar(int cID, Player owner) : base(cID, owner, 10, typeof(VicariousVictim))
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner == Owner }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (target.Position is GateCard positionGate && User.InDrop())
        {
            target.MoveFromFieldToDrop(positionGate.EnterOrder);
            User.MoveFromDropToField(positionGate);
            User.Boost(80 * Owner.BakuganDrop.Bakugans.Count, this);
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.InDrop() && user.Type == BakuganType.Griffon;

    [ModuleInitializer]
    internal static void Init() => Register(9, (cID, owner) => new RevivalRoar(cID, owner));
}
