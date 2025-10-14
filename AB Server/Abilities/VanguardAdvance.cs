using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class VanguardAdvance : AbilityCard
{
    public VanguardAdvance(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new YesNoSelector { Message = "INFO_WANTTARGET", ForPlayer = p => p == Owner },
            new GateSelector { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.Owner.TeamId != Owner.TeamId, Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
        ];
    }

    public override void Resolve()
    {
        User.Boost(50, this);
        base.Resolve();
    }

    public override void TriggerEffect()
    {
        if ((ResTargetSelectors[0] as YesNoSelector)!.IsYes)
            User.Move((ResTargetSelectors[1] as GateSelector)!.SelectedGate, new JObject() { ["MoveEffect"] = "Fireball" });
    }

    public override bool UserValidator(Bakugan user) =>
        user.JustEndedBattle && !user.BattleEndedInDraw && user.Position is GateCard posGate && posGate.BattleOver && user.IsAttribute(Attribute.Nova);

    [ModuleInitializer]
    internal static void Init() => Register(23, CardKind.NormalAbility, (cID, owner) => new VanguardAdvance(cID, owner, 23));
}
