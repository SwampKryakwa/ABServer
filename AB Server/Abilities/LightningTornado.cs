using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class LightningTornado : AbilityCard
    {
        public LightningTornado(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new YesNoSelector { ForPlayer = (p) => p == Owner, Message = "INFO_WANTTARGET" , Condition = () => Game.BakuganIndex.Any(x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower)},
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DECREASETARGET", TargetValidator = x => x.Position == User.Position && User.IsEnemyOf(x) && x.BasePower > User.BasePower, Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
            ];
        }

        public override void TriggerEffect() =>
            new LightningTornadoEffect(User, (ResTargetSelectors[1] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.IsAttribute(Attribute.Lumina) && user.InBattle;
    }

    internal class LightningTornadoEffect(Bakugan user, Bakugan target, int typeID, bool isCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan target = target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = isCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            // Increase the power of the user Bakugan by 100G
            User.Boost(new Boost(100), this);

            // If a target Bakugan is selected, decrease its power by 100G
            target?.Boost(new Boost(-100), this);
        }
    }
}
