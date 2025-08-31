namespace AB_Server.Abilities
{
    internal abstract partial class FusionAbility(int cID, Player owner, int typeId, Type baseAbilityType) : AbilityCard(cID, owner, typeId)
    {
        public static Func<int, Player, FusionAbility>[] FusionCtrs = Array.Empty<Func<int, Player, FusionAbility>>();

        internal static void Register(int typeId, Func<int, Player, FusionAbility> constructor)
        {
            if (FusionCtrs.Length <= typeId)
                Array.Resize(ref FusionCtrs, typeId + 1);
            FusionCtrs[typeId] = constructor;
        }

        public override CardKind Kind { get; } = CardKind.FusionAbility;
        public AbilityCard FusedTo;

        public override bool BakuganIsValid(Bakugan user) =>
            Owner.AbilityBlockers.Count == 0 && !user.Frenzied && user.IsPartner && IsActivateableByBakugan(user) && user.Owner == Owner;

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(baseAbilityType.IsInstanceOfType))
                ));
            Game.OnAnswer[Owner.Id] = PickUser;
        }

        public virtual void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void Activate()
        {
            FusedTo.Discard();

            EffectId = Game.NextEffectId++;

            Game.ThrowEvent(new()
            {
                ["Type"] = "AbilityAddedActiveZone",
                ["IsCopy"] = IsCopy,
                ["Id"] = EffectId,
                ["Card"] = TypeId,
                ["Kind"] = (int)Kind,
                ["User"] = User.BID,
                ["IsCounter"] = asCounter,
                ["Owner"] = Owner.Id
            });

            Game.ActiveZone.Add(this);
            Game.CardChain.Push(this);
            Game.CheckChain(Owner, this, User);
        }

        public override bool IsActivateable() =>
            Owner.BakuganOwned.Any(BakuganIsValid) && Owner.AbilityHand.Any(baseAbilityType.IsInstanceOfType);
    }
}
