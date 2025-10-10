using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class FrenziedFlames(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
{
    public override void TriggerEffect()
    {
        foreach (var bak in Owner.BakuganOwned.Where(x => x.OnField()))
            bak.Boost(300, this);
    }
    public override bool UserValidator(Bakugan user) =>
        user.JustEndedBattle && !user.BattleEndedInDraw && user.Position is GateCard posGate && posGate.BattleOver && user.IsAttribute(Attribute.Nova);

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(38, CardKind.NormalAbility, (cID, owner) => new FrenziedFlames(cID, owner, 38));
}
