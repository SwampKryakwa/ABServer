using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class JumpOver : AbilityCard
{
    public JumpOver(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacentVertically((User.Position as GateCard)!)}
        ];
    }

    public override void TriggerEffect()
    {
        GenericEffects.MoveBakuganEffect(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Zephyros);

    [ModuleInitializer]
    internal static void Init() => Register(33, CardKind.NormalAbility, (cID, owner) => new JumpOver(cID, owner, 33));
}
