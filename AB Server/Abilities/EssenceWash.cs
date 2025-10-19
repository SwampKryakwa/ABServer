using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class EssenceWash : AbilityCard
{
    public EssenceWash(int cId, Player owner, int typeId) : base(cId, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", Message = "INFO_ABILITY_TARGET", ForPlayer = p => p == Owner, TargetValidator = x => x.OnField() },
            new AttributeSelector() { Message = "INFO_PICKATTRIBUTE", ForPlayer = (p) => p == Owner, TargetValidator = (x) => ResTargetSelectors[0] is BakuganSelector bakuganSelector && (bakuganSelector.SelectedBakugan.CurrentAttributes.Count() != 1 || bakuganSelector.SelectedBakugan.CurrentAttributes.First() != x) }
        ];
    }

    public override void TriggerEffect()
    {
        (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan.ChangeAttribute((ResTargetSelectors[1] as AttributeSelector)!.SelectedAttribute, this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.OnField();

    public override bool ActivationCondition() =>
        Game.CurrentWindow == ActivationWindow.Normal && Owner.Bakugans.Count(x => x.IsAttribute(Attribute.Aqua)) >= 2;

    [ModuleInitializer]
    internal static void Init() => Register(31, CardKind.NormalAbility, (cID, owner) => new EssenceWash(cID, owner, 31));
}
