using AB_Server.Gates.SpecialGates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class MagmaProminence : AbilityCard
{
    public MagmaProminence(int cId, Player owner, int typeId) : base(cId, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField && !x.IsOpen }
        ];
    }

    public override void TriggerEffect()
    {
        GateOfSubterra80 transformedGate = new GateOfSubterra80(Game.GateIndex.Count, Owner);
        Game.GateIndex.Add(transformedGate);
        transformedGate.TransformFrom((CondTargetSelectors[0] as GateSelector)!.SelectedGate, false);
        Game.Field[transformedGate.Position.X, transformedGate.Position.Y] = transformedGate;
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Subterra) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(24, CardKind.NormalAbility, (cID, owner) => new MagmaProminence(cID, owner, 24));
}
