using System.Runtime.CompilerServices;

namespace AB_Server.Abilities.Fusions;

internal class PowerAccord : FusionAbility
{
    public PowerAccord(int cID, Player owner) : base(cID, owner, 11, typeof(CommandConvergence))
    {
        ResTargetSelectors =
        [
            new AttributeSelector() { Message = "INFO_PICKATTRIBUTE", ForPlayer = (p) => p == Owner, TargetValidator = (x) => User.CurrentAttributes.Count() != 1 || User.CurrentAttributes.First() != x }
        ];
    }

    public override void TriggerEffect()
    {
        User.ChangeAttribute((ResTargetSelectors[0] as AttributeSelector)!.SelectedAttribute, this);
        User.Boost(new Boost((short)(80 * Game.BakuganIndex.Count(x => x.OnField() && x.IsAttribute((ResTargetSelectors[0] as AttributeSelector)!.SelectedAttribute)))), this);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.Type == BakuganType.Knight && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(10, (cID, owner) => new PowerAccord(cID, owner));
}
