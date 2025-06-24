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
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if ((target.Position as GateCard)!.IsAdjacent(User.Position as GateCard))
                new MoveBakuganEffect(User, target, User.Position as GateCard, TypeId, (int)Kind, new JObject { ["MoveEffect"] = "LightningChain", ["EffectSource"] = User.BID }, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(target => target.Owner != Owner && target.Position is GateCard targetGate && user.Position is GateCard userGate && userGate != targetGate) && Game.GateIndex.Any((user.Position as GateCard).IsAdjacent);

        public bool ValidTarget(Bakugan bakugan) =>
            bakugan.Owner != Owner && bakugan.Position is GateCard targetGate && User.Position is GateCard userGate && userGate != targetGate;
    }
}
