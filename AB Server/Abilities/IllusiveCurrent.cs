using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class IllusiveCurrent : AbilityCard
    {
        public IllusiveCurrent(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BH", ForPlayer = owner.Id, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner &&  User.IsAttribute(Attribute.Aqua)
                ?  x.InHand()
                :  x.InHand() && x.MainAttribute == Attribute.Aqua}
            ];
        }

        public override void TriggerEffect() => new IllusiveCurrentEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && ((user.IsAttribute(Attribute.Aqua) && Owner.Bakugans.Count != 0) || user.Owner.Bakugans.Any(x => x.IsAttribute(Attribute.Aqua)));

        public static new bool HasValidTargets(Bakugan user) => user.OnField();
    }

    internal class IllusiveCurrentEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan selectedBakugan = selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            if (User.Position is GateCard positionGate && selectedBakugan.Position is Player)
            {
                User.ToHand(positionGate.EnterOrder);
                selectedBakugan.AddFromHand(positionGate);
            }
        }
    }
}

