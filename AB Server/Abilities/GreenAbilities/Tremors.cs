using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class Tremors(int cID, Player owner) : FusionAbility(cID, owner, 5, typeof(NoseSlap))
{
    public override void TriggerEffect()
    {
        foreach (var target in Game.GateIndex.Where((User.Position as GateCard)!.IsDiagonal))
            foreach (var bakugan in target.Bakugans)
                bakugan.Boost(-bakugan.Power, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Elephant && user.OnField();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Intermediate;

    [ModuleInitializer]
    internal static void Init() => Register(6, (cID, owner) => new Tremors(cID, owner));
}
