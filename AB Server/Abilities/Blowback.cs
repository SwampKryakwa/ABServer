using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Blowback(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        if (User.Position is GateCard positionGate)
            User.MoveFromFieldToHand(positionGate.EnterOrder);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Owner.BakuganOwned.Any(b => b.IsAttribute(Attribute.Zephyros)) && user.OnField() && Game.CurrentWindow == ActivationWindow.Normal;

    [ModuleInitializer]
    internal static void Init() => Register(5, CardKind.NormalAbility, (cID, owner) => new Blowback(cID, owner, 5));
}
