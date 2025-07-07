using AB_Server.Abilities.Fusions;

namespace AB_Server.Abilities
{
    internal class FusionAbility(int cID, Player owner, int typeId, Type baseAbilityType) : AbilityCard(cID, owner, typeId)
    {
        public static Func<int, Player, FusionAbility>[] FusionCtrs =
        [
            (cID, owner) => new BruteUltimatum(cID, owner),
            (cID, owner) => new PinDown(cID, owner),
            (cID, owner) => new Marionette(cID, owner),
            (cID, owner) => new StrikeBack(cID, owner),
            (cID, owner) => new DoubleDimension(cID, owner),
            (cID, owner) => new SaurusRage(cID, owner),
            (cID, owner) => new Tremors(cID, owner),
            (cID, owner) => new CutInSaber(cID, owner),
            (cID, owner) => new CoreLinkage(cID, owner),
            (cID, owner) => new RevivalRoar(cID, owner),
            (cID, owner) => new PowerAccord(cID, owner),
            (cID, owner) => new GrandDevourer(cID, owner),
            (cID, owner) => new SilentPact(cID, owner),
            (cID, owner) => new HarmonicGrace(cID, owner)
        ];

        public override CardKind Kind { get; } = CardKind.FusionAbility;
        public AbilityCard FusedTo;
        bool asCounter;

        public override bool BakuganIsValid(Bakugan user) =>
            Owner.AbilityBlockers.Count == 0 && !user.Frenzied && user.IsPartner && IsActivateableByBakugan(user) && user.Owner == Owner;

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(baseAbilityType.IsInstanceOfType))
                ));
            Game.OnAnswer[Owner.Id] = PickUser;
        }

        public virtual void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
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
