using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CutInSaber : FusionAbility
    {
        public CutInSaber(int cID, Player owner) : base(cID, owner, 3, typeof(CrystalFang))
        {
            TargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_GATETARGET", TargetValidator = ValidTarget}
            ];
        }

        public override void PickUser()
        {
            FusedTo = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.HandBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect() =>
            new CutInSaberEffect(User, (TargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleStart && user.Type == BakuganType.Tigress && user.InHand() && Game.GateIndex.Any(ValidTarget);

        public bool ValidTarget(GateCard gateCard) =>
            gateCard.Bakugans.Count >= 2 && gateCard.Freezing.Count == 0;
    }

    internal class CutInSaberEffect
    {
        public int TypeId { get; }
        Bakugan user;
        GateCard targetGate;
        Game game { get => user.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public CutInSaberEffect(Bakugan user, GateCard targetGate, int typeID, bool IsCopy)
        {
            this.user = user;
            this.targetGate = targetGate;
            this.IsCopy = IsCopy;

            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
                game.NewEvents[i].Add(EventBuilder.ActivateAbilityEffect(TypeId, 1, user));

            if (user.InHand())
                user.AddFromHand(targetGate);
        }
    }
}
