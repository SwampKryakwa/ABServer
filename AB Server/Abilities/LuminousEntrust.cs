using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class LuminousEntrust : AbilityCard
    {
        public LuminousEntrust(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            ResTargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.InHand() && x.Owner == Owner },
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = x => x.OnField }
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
            var bakTarget = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            bakTarget.AddFromHandToField((CondTargetSelectors[1] as GateSelector)!.SelectedGate);
            bakTarget.Boost(50, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Intermediate && user.IsAttribute(Attribute.Lumina) && user.InDrop() && Game.GateIndex.Any(x => x.OnField) && user.Owner.Bakugans.Count != 0;

        [ModuleInitializer]
        internal static void Init() => Register(52, CardKind.NormalAbility, (cID, owner) => new LuminousEntrust(cID, owner, 52));
    }
}