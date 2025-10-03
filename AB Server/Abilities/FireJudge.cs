using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class FireJudge(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
        new ContinuousBoostUntilDestroyedEffect(User, User, 100, TypeId, Kind, IsCopy).Activate();

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Nova) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(0, CardKind.NormalAbility, (cID, owner) => new FireJudge(cID, owner, 0));
}