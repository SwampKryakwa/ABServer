using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class VanguardAdvance : AbilityCard
{
    public VanguardAdvance(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        CondTargetSelectors =
        [
            new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacent((User.Position as GateCard)!) && x.Owner.TeamId != Owner.TeamId }
        ];
    }

    public override void TriggerEffect() =>
        GenericEffects.MoveBakuganEffect(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, new JObject() { ["MoveEffect"] = "Fireball" });

    public override bool IsActivateableByBakugan(Bakugan user) =>
        user.Position is GateCard positionGate && positionGate.BattleOver && user.IsAttribute(Attribute.Nova) && Game.GateIndex.Any(x => positionGate.IsAdjacent(x) && x.Bakugans.Any(user.IsOpponentOf));

    public static new bool HasValidTargets(Bakugan user) =>
        user.Game.GateIndex.Any(x => x.IsAdjacent((user.Position as GateCard)!) && x.Bakugans.Any(user.IsOpponentOf));

    [ModuleInitializer]
    internal static void Init() => Register(23, CardKind.NormalAbility, (cID, owner) => new VanguardAdvance(cID, owner, 23));
}
