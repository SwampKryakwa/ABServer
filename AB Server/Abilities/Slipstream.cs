using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Slipstream(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        Owner.RemainingThrows++;
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && Game.TurnPlayer == Owner.Id && user.OnField() && user.IsAttribute(Attribute.Zephyros);

    [ModuleInitializer]
    internal static void Init() => Register(41, CardKind.NormalAbility, (cID, owner) => new Slipstream(cID, owner, 41));
}
