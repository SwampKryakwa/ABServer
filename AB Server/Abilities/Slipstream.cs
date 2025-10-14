using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Slipstream(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        Owner.AllowedThrows = 2;
    }

    public override bool UserValidator(Bakugan user) =>
        Game.TurnPlayer == Owner.PlayerId && user.OnField() && user.IsAttribute(Attribute.Zephyros);

    [ModuleInitializer]
    internal static void Init() => Register(41, CardKind.NormalAbility, (cID, owner) => new Slipstream(cID, owner, 41));
}
