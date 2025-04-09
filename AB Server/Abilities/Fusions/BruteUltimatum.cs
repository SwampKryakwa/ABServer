using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BruteUltimatum : FusionAbility
    {
        public BruteUltimatum(int cID, Player owner) : base(cID, owner, 7, typeof(MercilessTriumph))
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = owner.Id == 1 ? 0 : 1, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.InHand() && x.Owner != Owner}
            ];
        }

        public override void TriggerEffect() =>
            new BruteUltimatumEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Glorius && user.OnField() && user.JustEndedBattle && !user.BattleEndedInDraw;
    }

    internal class BruteUltimatumEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public BruteUltimatumEffect(Bakugan user, Bakugan target, int typeID)
        {
            this.user = user;
            this.target = target;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (user.Position is GateCard positionGate)
                target.AddFromHand(positionGate);
        }
    }
}
