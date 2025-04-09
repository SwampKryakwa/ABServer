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
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_MOVETARGET", TargetValidator = b => b.Owner != Owner && b.Position is GateCard targetGate && User.Position is GateCard userGate && userGate != targetGate},
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x != (TargetSelectors[0] as BakuganSelector).SelectedBakugan.Position}
            ];
        }

        public override void TriggerEffect() =>
            new MarionetteEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, (TargetSelectors[1] as GateSelector).SelectedGate, TypeId).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Mantis && user.IsPartner && user.OnField() && Game.BakuganIndex.Any(x => x.Position != user.Position && x.OnField());
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
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (targetBakugan.OnField())
                targetBakugan.Move(targetGate, MoveSource.Effect);
        }
    }
}
