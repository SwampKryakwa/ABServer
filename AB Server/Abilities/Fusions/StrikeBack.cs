using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
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
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.GraveBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
                new StrikeBackEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.InGrave() && user.Type == BakuganType.Raptor && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.IsEnemyOf(user));
    }

    internal class StrikeBackEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan user = user;
        Bakugan target = target;
        Game game { get => user.Game; }

        public Player Onwer { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));


            if (user.InGrave())
            {
                user.FromGrave((target.Position as GateCard));
                user.Boost(new Boost((short)(target.Power - user.Power + 10)), this);
            }
        }
    }
}
