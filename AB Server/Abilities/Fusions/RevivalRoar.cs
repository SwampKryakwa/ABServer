using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class RevivalRoar : FusionAbility
    {
        public RevivalRoar(int cID, Player owner) : base(cID, owner, 10, typeof(VicariousVictim))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner == Owner }
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.DropBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (target.Position is GateCard positionGate && User.InDrop())
            {
                target.MoveFromFieldToDrop(positionGate.EnterOrder);
                User.MoveFromDropToField(positionGate);
                foreach (var bakugan in Game.BakuganIndex.Where(x => x.OnField() && x.Owner.TeamId != Owner.TeamId))
                {
                    bakugan.Boost(new Boost((short)(Owner.BakuganDrop.Bakugans.Count * -80)), this);
                }
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.InDrop() && user.Type == BakuganType.Griffon && Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner);
    }
}
