using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Fusions
{
    internal class Tremors : FusionAbility
    {
        public Tremors(int cID, Player owner)
        {
            TypeId = 5;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BaseAbilityType = typeof(NoseSlap);
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYUSER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
            ));

            Game.AwaitingAnswers[Owner.Id] = PickTargets;
        }

        public void PickTargets()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            var validTargets = Game.BakuganIndex.Where(b => b.Owner != Owner && b.Position is GateCard targetGate && !(User.Position as GateCard).IsTouching(targetGate));

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_SELECT_TARGETS", TypeId, (int)Kind, validTargets)
            ));

            Game.AwaitingAnswers[Owner.Id] = HandleTargetSelection;
        }

        private List<Bakugan> selectedTargets = new();

        public void HandleTargetSelection()
        {
            var selectedBakugan = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            selectedTargets.Add(selectedBakugan);

            var validTargets = Game.BakuganIndex.Where(b => b.Owner != Owner && b.Position is GateCard targetGate && !(User.Position as GateCard).IsTouching(targetGate) && !selectedTargets.Contains(b));

            if (validTargets.Any())
            {
                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.BoolSelectionEvent("INFO_SELECT_ANOTHER_BAKUGAN")
                ));

                Game.AwaitingAnswers[Owner.Id] = HandleAnotherTargetSelection;
            }
            else
            {
                Activate();
            }
        }

        public void HandleAnotherTargetSelection()
        {
            if ((bool)Game.IncomingSelection[Owner.Id]["array"][0]["answer"])
            {
                var validTargets = Game.BakuganIndex.Where(b => b.Owner != Owner && b.Position is GateCard targetGate && !(User.Position as GateCard).IsTouching(targetGate) && !selectedTargets.Contains(b));

                Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                    EventBuilder.FieldBakuganSelection("INFO_SELECT_TARGETS", TypeId, (int)Kind, validTargets)
                ));

                Game.AwaitingAnswers[Owner.Id] = HandleTargetSelection;
            }
            else
            {
                Activate();
            }
        }

        public new void Activate()
        {
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

        public override void Resolve()
        {
            if (!counterNegated)
                new TremorsEffect(User, selectedTargets, TypeId, IsCopy).Activate();

            Dispose();
        }

        public override void DoubleEffect() =>
            new TremorsEffect(User, selectedTargets, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Type == BakuganType.Elephant && user.OnField();
    }

    internal class TremorsEffect
    {
        public int TypeId { get; }
        Bakugan user;
        List<Bakugan> targets;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public TremorsEffect(Bakugan user, List<Bakugan> targets, int typeID, bool IsCopy)
        {
            this.user = user;
            this.targets = targets;
            
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "FusionAbilityActivateEffect" },
                    { "Kind", 1 },
                    { "Card", TypeId },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Treatment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            foreach (var target in targets)
            {
                if (target.Power < user.Power)
                {
                    // Destroy the target Bakugan if it is on the field
                    if (target.Position is GateCard positionGate)
                        target.DestroyOnField(positionGate.EnterOrder);
                }
            }
        }
    }
}
