using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class BruteUltimatum : FusionAbility
{
    public BruteUltimatum(int cID, Player owner) : base(cID, owner, 7, typeof(MercilessTriumph))
    {
        CondTargetSelectors =
        [
            new PlayerSelector { ForPlayer = p => p == Owner, Message = "INFO_PICK_PLAYER", TargetValidator = p => p.TeamId != Owner.TeamId && p.Bakugans.Count != 0 }
        ];
        ResTargetSelectors =
        [
            new BakuganSelector() { ClientType = "BH", ForPlayer = p => p == (CondTargetSelectors[0] as PlayerSelector)!.SelectedPlayer, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.InHand() && x.Owner != Owner, Condition = () => (CondTargetSelectors[0] as PlayerSelector)!.SelectedPlayer.Bakugans.Count != 0 }
        ];
    }

    public override void TriggerEffect()
    {
        var target = (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
        if (User.Position is GateCard positionGate)
            target?.AddFromHandToField(positionGate);
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.Type == BakuganType.Glorius && user.Position is GateCard posGate && posGate.BattleOver && user.JustEndedBattle && !user.BattleEndedInDraw && Game.Players.Any(p => p.TeamId != Owner.TeamId && p.Bakugans.Count != 0);

    [ModuleInitializer]
    internal static void Init() => Register(0, (cID, owner) => new BruteUltimatum(cID, owner));
}
