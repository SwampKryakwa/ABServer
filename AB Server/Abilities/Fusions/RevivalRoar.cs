using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class RevivalRoar : FusionAbility
    {
        public RevivalRoar(int cID, Player owner) : base(cID, owner, 10, typeof(VicariousVictim))
        {
            TargetSelectors =
                [
                    new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner == Owner}
                ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.GraveBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
                new RevivalRoarEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.InGrave() && user.Type == BakuganType.Griffon && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner);
    }

    internal class RevivalRoarEffect(Bakugan user, Bakugan target, int typeID, bool isCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan user = user;
        Bakugan target = target;
        Game game { get => user.Game; }

        Player owner { get => user.Owner; }
        bool IsCopy = isCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (target.Position is GateCard positionGate && user.InGrave())
            {
                target.DestroyOnField(positionGate.EnterOrder);
                user.FromGrave(positionGate);
                user.Boost(new Boost((short)(owner.BakuganGrave.Bakugans.Count * 100)), this);
            }
        }
    }
}
