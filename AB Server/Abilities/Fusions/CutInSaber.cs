using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class CutInSaber : FusionAbility
    {
        public CutInSaber(int cID, Player owner) : base(cID, owner, 3, typeof(CrystalFang))
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = gateCard => gateCard.IsBattleGoing && gateCard.Bakugans.Any(User.IsOpponentOf)}
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect()
        {
            if (User.InHand())
                User.AddFromHand((CondTargetSelectors[0] as GateSelector)!.SelectedGate);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.Type == BakuganType.Tigress && (user.InHand() || user.OnField()) && Game.GateIndex.Any(gateCard => gateCard.IsBattleGoing && gateCard.Bakugans.Any(user.IsOpponentOf));
    }
}