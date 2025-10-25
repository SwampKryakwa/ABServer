using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Steadfast(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        User.Boost(200, this);
        Owner.AllowedThrows = 0;
    }

    public override bool UserValidator(Bakugan user) =>
        user.IsAttribute(Attribute.Subterra) && user.OnField();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.TurnStart;

    [ModuleInitializer]
    internal static void Init() => Register(55, CardKind.NormalAbility, (cID, owner) => new Steadfast(cID, owner, 55));
}