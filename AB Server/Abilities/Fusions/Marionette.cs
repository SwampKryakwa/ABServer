using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AB_Server.Abilities
{
    internal class Marionette : FusionAbility
    {
        public Marionette(int cID, Player owner) : base(cID, owner, 6, typeof(SlingBlazer))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = ValidTarget}
            ];

            ResTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x != (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan.Position && (User.Position as GateCard).IsAdjacent(x)}
            ];
        }

        public override void TriggerEffect()
        {
            if ((ResTargetSelectors[0] as GateSelector).SelectedGate != null)
                new MoveBakuganEffect(User, (CondTargetSelectors[0] as BakuganSelector).SelectedBakugan, (ResTargetSelectors[0] as GateSelector).SelectedGate, TypeId, (int)Kind, new JObject { ["MoveEffect"] = "LightningChain", ["EffectSource"] = User.BID }, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(target => target.Owner != Owner && target.Position is GateCard targetGate && user.Position is GateCard userGate && userGate != targetGate) && Game.GateIndex.Any((user.Position as GateCard).IsAdjacent);

        public bool ValidTarget(Bakugan bakugan) =>
            bakugan.Owner != Owner && bakugan.Position is GateCard targetGate && User.Position is GateCard userGate && userGate != targetGate;
    }
}
