using AB_Server.Gates;

namespace AB_Server.Abilities
{
    internal class DefiantCounterattack : AbilityCard
    {
        public DefiantCounterattack(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.OnField && x.Bakugans.Any(User.IsOpponentOf) }
            ];
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.DropBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect()
        {
            if (User.InDrop() && (ResTargetSelectors[0] as GateSelector)!.SelectedGate is GateCard targetGate && targetGate.OnField)
                User.MoveFromDropToField(targetGate);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Intermediate && user.Type == BakuganType.Raptor && user.InDrop();

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(gate => gate.BattleOver);
    }
}

