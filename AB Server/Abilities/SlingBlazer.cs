using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class SlingBlazer : AbilityCard
    {
        public SlingBlazer(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.InBattle && x.Owner.TeamId != Owner.TeamId}
            ];
            ResTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacentHorizontally(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
            new MoveBakuganEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, (ResTargetSelectors[0] as GateSelector).SelectedGate, TypeId, (int)Kind, new JObject() { ["MoveEffect"] = "LightningChain", ["EffectSource"] = User.BID }, IsCopy);

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.InBattle && Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget)) && Game.GateIndex.Any(x => x.IsAdjacentHorizontally(user.Position as GateCard));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsEnemyOf(possibleTarget));
    }
}
