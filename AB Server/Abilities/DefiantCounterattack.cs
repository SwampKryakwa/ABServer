using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class DefiantCounterattack : AbilityCard
    {
        public DefiantCounterattack(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.OnField && x.Bakugans.Any(User.IsEnemyOf)}
            ];
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.GraveBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
                new DefiantCounterattackEffect(User, (ResTargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Raptor && user.InGrave();

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(gate => gate.BattleOver);
    }
    internal class DefiantCounterattackEffect
    {
        public int TypeId { get; }
        Bakugan User;
        GateCard battleGate;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public DefiantCounterattackEffect(Bakugan user, GateCard battleGate, int typeID, bool IsCopy)
        {
            User = user;
            this.battleGate = battleGate;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            User.FromGrave(battleGate);
        }
    }
}

