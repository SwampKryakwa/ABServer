using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class LightningTornado : AbilityCard
    {
        public LightningTornado(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET" , Condition = () => Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsOpponentOf(x) && x.BasePower > User.BasePower)},
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DECREASETARGET", TargetValidator = x => x.Position == User.Position && User.IsOpponentOf(x) && x.BasePower > User.BasePower, Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
            ];
        }

        public override void TriggerEffect()
        {
            // Increase the power of the user Bakugan by 100G
            User.Boost(new Boost(100), this);

            // If a target Bakugan is selected, decrease its power by 100G
            (ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan?.Boost(new Boost(-100), this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Lumina) && user.InBattle;
    }
}
