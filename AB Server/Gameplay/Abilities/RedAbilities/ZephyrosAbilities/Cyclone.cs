using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class Cyclone : AbilityCard
{
    public Cyclone(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsOpponentOf(User) }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (User.InHand())
            target.Boost(Owner.Bakugans.Count(x => x.IsAttribute(Attribute.Zephyros)) * -80, this);
        else if (User.OnField())
            target.Boost(Game.BakuganIndex.Count(x => x.OnField() && x.Owner == Owner && x.IsAttribute(Attribute.Zephyros)) * -80, this);
    }

    public override bool UserValidator(Bakugan user) =>
        (user.OnField() || user.InHand()) && user.IsAttribute(Attribute.Zephyros);

    [ModuleInitializer]
    internal static void Init() => Register(43, CardKind.NormalAbility, (cID, owner) => new Cyclone(cID, owner, 43));
}
