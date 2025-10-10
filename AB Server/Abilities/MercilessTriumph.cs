using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class MercilessTriumph : AbilityCard
{
    public MercilessTriumph(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = target => target.OnField() && target != User }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        target?.Boost(new Boost((short)-target.Power), this);
    }

    public override bool UserValidator(Bakugan user) =>
        user.Type == BakuganType.Glorius && user.Position is GateCard posGate && posGate.BattleOver && user.JustEndedBattle && !user.BattleEndedInDraw;

    [ModuleInitializer]
    internal static void Init() => Register(8, CardKind.NormalAbility, (cID, owner) => new MercilessTriumph(cID, owner, 8));
}




