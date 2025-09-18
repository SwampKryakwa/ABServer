using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class Cyclone : AbilityCard
    {
        public Cyclone(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.OnField() && x.Owner.TeamId != Owner.TeamId }
            ];
        }

        public override void Setup(bool asCounter)
        {
            this.asCounter = asCounter;
            Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(!asCounter && Game.CurrentWindow == ActivationWindow.Normal,
                EventBuilder.AnyBakuganSelection("INFO_ABILITY_USER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

            Game.OnAnswer[Owner.Id] = RecieveUser;
        }

        public override void TriggerEffect()
        {
            var target = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            if (User.InHand())
                target.Boost(Owner.Bakugans.Count(x => x.IsAttribute(Attribute.Zephyros)) * -80, this);
            else if (User.OnField())
                target.Boost(Game.BakuganIndex.Count(x => x.OnField() && x.Owner == Owner && x.IsAttribute(Attribute.Zephyros)) * -80, this);
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && (user.OnField() || user.InHand()) && user.IsAttribute(Attribute.Zephyros) && Game.BakuganIndex.Any(x => x.OnField() && x.IsOpponentOf(user));

        [ModuleInitializer]
        internal static void Init() => Register(43, CardKind.NormalAbility, (cID, owner) => new Cyclone(cID, owner, 43));
    }
}
