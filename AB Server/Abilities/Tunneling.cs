using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Tunneling : AbilityCard
    {
        public Tunneling(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x.Position.X == (User.Position as GateCard).Position.X && x != User.Position && !x.IsTouching(User.Position as GateCard)}
            ];
        }

        public override void TriggerEffect() =>
                new TunnelingEffect(User, (TargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.IsAttribute(Attribute.Subterra) && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(gate => gate.OnField && gate.Position.X == (user.Position as GateCard).Position.X && !gate.IsTouching(user.Position as GateCard));
    }

    internal class TunnelingEffect(Bakugan user, GateCard moveTarget, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        Bakugan User = user;
        GateCard moveTarget = moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            User.Move(moveTarget, new JObject() { ["MoveEffect"] = "Submerge" });
        }
    }
}
