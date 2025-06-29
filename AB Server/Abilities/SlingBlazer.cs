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
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = x => x.Position == User.Position && x.IsOpponentOf(User) }
            ];
            ResTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.IsAdjacentHorizontally(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
            new MoveBakuganEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, (ResTargetSelectors[0] as GateSelector)!.SelectedGate, TypeId, (int)Kind, new JObject() { ["MoveEffect"] = "LightningChain", ["Attribute"] = (int)User.BaseAttribute, ["EffectSource"] = User.BID }, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.InBattle && Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsOpponentOf(possibleTarget)) && Game.GateIndex.Any(x => x.IsAdjacentHorizontally(user.Position as GateCard));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(possibleTarget => possibleTarget.InBattle && user.IsOpponentOf(possibleTarget));
    }
}
