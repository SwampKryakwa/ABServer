using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class BruteUltimatum : FusionAbility
    {
        public BruteUltimatum(int cID, Player owner) : base(cID, owner, 7, typeof(MercilessTriumph))
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p.TeamId != Owner.TeamId, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.InHand() && x.Owner != Owner}
            ];
        }

        public override void TriggerEffect() =>
            new BruteUltimatumEffect(User, (ResTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId).Activate();

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
            

            if (user.Position is GateCard positionGate)
                target.AddFromHand(positionGate);
        }
    }
}
