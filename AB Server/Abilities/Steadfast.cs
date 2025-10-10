using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Steadfast(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
        Owner.AllowedThrows = 0;

    public override bool UserValidator(Bakugan user) =>
        user.IsAttribute(Attribute.Subterra) && user.InHand() && Owner.UsedThrows == 0;

    [ModuleInitializer]
    internal static void Init() => Register(55, CardKind.NormalAbility, (cID, owner) => new Steadfast(cID, owner, 55));
}