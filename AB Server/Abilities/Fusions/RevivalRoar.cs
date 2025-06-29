using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class RevivalRoar : FusionAbility
    {
        public RevivalRoar(int cID, Player owner) : base(cID, owner, 10, typeof(VicariousVictim))
        {
            CondTargetSelectors =
                [
                    new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner == Owner}
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

        public override void TriggerEffect() =>
                new RevivalRoarEffect(User, (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.InDrop() && user.Type == BakuganType.Griffon && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner);
    }

    internal class RevivalRoarEffect(Bakugan user, Bakugan target, int typeID, bool isCopy)
    {
        Player owner { get => user.Owner; }

        public void Activate()
        {
            if (target.Position is GateCard positionGate && user.InDrop())
            {
                target.DestroyOnField(positionGate.EnterOrder);
                user.FromDrop(positionGate);
                user.Boost(new Boost((short)(owner.BakuganDrop.Bakugans.Count * 100)), this);
            }
        }
    }
}
