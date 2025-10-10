using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class BloomOfAgony(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect()
    {
        foreach (Bakugan target in Game.BakuganIndex.Where(x => x.OnField()))
            target.Boost(-300, this);
    }

    public override bool UserValidator(Bakugan user) => user.Position is GateCard posGate && posGate.BattleStarting && user.IsAttribute(Attribute.Darkon);

    public override bool ActivationCondition() => Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(12, CardKind.NormalAbility, (cID, owner) => new BloomOfAgony(cID, owner, 12));
}
