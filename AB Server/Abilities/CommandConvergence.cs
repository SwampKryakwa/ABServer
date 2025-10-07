using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class CommandConvergence : AbilityCard
{
    public CommandConvergence(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new AttributeSelector() { Message = "INFO_PICKATTRIBUTE", ForPlayer = (p) => p == Owner, TargetValidator = (x) => true }
        ];
    }

    public override void TriggerEffect()
    {
        foreach (var target in Game.BakuganIndex.Where(x => x.OnField() && x.IsAttribute((Attribute)(ResTargetSelectors[0] as OptionSelector)!.SelectedOption)))
            target.Boost(120, this);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Knight && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(34, CardKind.NormalAbility, (cID, owner) => new CommandConvergence(cID, owner, 34));
}
