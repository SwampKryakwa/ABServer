using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class CutInSaber : FusionAbility
    {
        public CutInSaber(int cID, Player owner) : base(cID, owner, 3, typeof(CrystalFang))
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = gateCard => gateCard.IsBattleGoing && gateCard.Bakugans.Any(User.IsEnemyOf)}
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
            new CutInSaberEffect(User, (CondTargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.Type == BakuganType.Tigress && (user.InHand() || user.OnField()) && Game.GateIndex.Any(gateCard => gateCard.IsBattleGoing && gateCard.Bakugans.Any(user.IsEnemyOf));
    }

    internal class CutInSaberEffect
    {
        public int TypeId { get; }
        Bakugan user;
        GateCard targetGate;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public CutInSaberEffect(Bakugan user, GateCard targetGate, int typeID, bool IsCopy)
        {
            this.user = user;
            this.targetGate = targetGate;
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (user.InHand())
                user.AddFromHand(targetGate);
        }
    }
}
