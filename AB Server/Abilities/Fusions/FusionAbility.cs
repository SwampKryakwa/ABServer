using AB_Server.Abilities.Fusions;

namespace AB_Server.Abilities
{
    internal class FusionAbility : AbilityCard
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
            (cID, owner) => new CutInSaber(cID, owner)
        ];
        public override AbilityKind Kind { get; } = AbilityKind.FusionAbility;
        public Type BaseAbilityType;
        public AbilityCard FusedTo;
        bool asCounter;

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.AbilitySelection("INFO_FUSIONBASE", Owner.AbilityHand.Where(BaseAbilityType.IsInstanceOfType))
                ));
            Game.AwaitingAnswers[Owner.Id] = PickUser;
        }

        public virtual void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public new void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            FusedTo.Discard();

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
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
            }
            Game.CheckChain(Owner, this, User);
        }

        public override bool IsActivateable() =>
            Owner.BakuganOwned.Any(BakuganIsValid) && Owner.AbilityHand.Any(BaseAbilityType.IsInstanceOfType);
    }
}
