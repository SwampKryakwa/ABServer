using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Marionette : FusionAbility
    {
        public Marionette(int cID, Player owner) : base(cID, owner, 6, typeof(SlingBlazer))
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = ValidTarget},
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x != (TargetSelectors[0] as BakuganSelector).SelectedBakugan.Position}
            ];
        }

        public override void TriggerEffect() =>
            new MarionetteEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, (TargetSelectors[1] as GateSelector).SelectedGate, TypeId).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(target => target.Owner != Owner && target.Position is GateCard targetGate && user.Position is GateCard userGate && userGate != targetGate);

        public bool ValidTarget(Bakugan bakugan) =>
            bakugan.Owner != Owner && bakugan.Position is GateCard targetGate && User.Position is GateCard userGate && userGate != targetGate;
    }

    internal class MarionetteEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan targetBakugan;
        GateCard targetGate;
        Game game { get => targetBakugan.Game; }

        public MarionetteEffect(Bakugan user, Bakugan targetBakugan, GateCard targetGate, int typeID)
        {
            this.user = user;
            this.targetBakugan = targetBakugan;
            this.targetGate = targetGate;
            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (targetBakugan.OnField())
                targetBakugan.Move(targetGate, new JObject() { ["MoveEffect"] = "LightningChain", ["EffectSource"] = user.BID });
        }
    }
}
