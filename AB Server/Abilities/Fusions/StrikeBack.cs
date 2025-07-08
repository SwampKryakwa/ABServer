using AB_Server.Gates;

namespace AB_Server.Abilities.Fusions
{
    internal class StrikeBack : FusionAbility
    {
        public StrikeBack(int cID, Player owner) : base(cID, owner, 2, typeof(DefiantCounterattack))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.IsOpponentOf(User)}
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
            if (User.InDrop())
            {
                User.MoveFromDropToField((target.Position as GateCard)!);
                User.Boost((short)(target.Power - User.Power + 10), this);
            }
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.InDrop() && user.Type == BakuganType.Raptor && user.IsPartner && Game.BakuganIndex.Any(x => x.OnField() && x.IsOpponentOf(user));
    }
}
