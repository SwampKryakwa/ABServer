using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class StrikeBack : FusionAbility
    {
        public StrikeBack(int cID, Player owner) : base(cID, owner, 2, typeof(DefiantCounterattack))
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsEnemyOf(User)}
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
                new StrikeBackEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.InGrave() && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.IsEnemyOf(user));
    }

    internal class StrikeBackEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Bakugan target;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public StrikeBackEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            this.user = user;
            this.target = target;
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));


            if (user.InGrave())
            {
                user.FromGrave((target.Position as GateCard));
                user.Boost(new Boost((short)(target.Power - user.Power + 10)), this);
            }
        }
    }
}
