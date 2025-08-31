using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class CutInSaber : FusionAbility
    {
        public CutInSaber(int cID, Player owner) : base(cID, owner, 3, typeof(CrystalFang))
        {
            CondTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = possibleTarget => possibleTarget.Owner != Owner && possibleTarget.Position is GateCard posGate && posGate.BattleStarting }
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.PlayerAnswers[Owner.Id]!["array"][0]["ability"]];

            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.InHand() && target.Position is GateCard posGate)
                User.AddFromHandToField(posGate);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
        user.Type == BakuganType.Tigress && user.InHand() && Game.GateIndex.Any(gateCard => gateCard.BattleStarting && gateCard.Bakugans.Any(user.IsOpponentOf));

        [ModuleInitializer]
        internal static void Init() => FusionAbility.Register(7, (cID, owner) => new CutInSaber(cID, owner));
    }
}