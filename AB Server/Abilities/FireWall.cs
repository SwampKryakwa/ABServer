using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class FireWall : AbilityCard
{
    public FireWall(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() }
        ];

        ResTargetSelectors =
        [
            new OptionSelector() { Message = "INFO_PICKER_FIREWALL", ForPlayer = (p) => p == Owner, OptionCount = 2}
        ];
    }

    public override void TriggerEffect()
    {
        var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if ((ResTargetSelectors[0] as OptionSelector)!.SelectedOption == 0)
            target.Boost(new Boost((short)-target.AdditionalPower), this);
        else
            target.Boost(new Boost(-50), this);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Intermediate && user.Position is GateCard posGate && posGate.BattleOver && Owner.BakuganOwned.Any(x => x.IsAttribute(Attribute.Nova));

    public static new bool HasValidTargets(Bakugan user) =>
        user.Position.Bakugans.Any(x => x.Owner != user.Owner);

    [ModuleInitializer]
    internal static void Init() => Register(9, CardKind.NormalAbility, (cID, owner) => new FireWall(cID, owner, 9));
}

