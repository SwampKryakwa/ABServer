using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class OreganoMurder : AbilityCard
{
    public OreganoMurder(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET", Condition = () => Owner.Bakugans.Count == 0 && User.OnField() && Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) },
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Position == User.Position && x.IsOpponentOf(User), Condition = () => Owner.Bakugans.Count == 0 && User.OnField() && Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x)) }
        ];
    }

    public override void Resolve()
    {
        User.Boost(100, this);
        base.Resolve();
    }

    public override void TriggerEffect() =>
        (ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan?.Boost(-100, this);

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Darkon);

    [ModuleInitializer]
    internal static void Init() => Register(49, CardKind.NormalAbility, (cID, owner) => new OreganoMurder(cID, owner, 49));
}
