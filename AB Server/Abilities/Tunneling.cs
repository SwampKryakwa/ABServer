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
            user.Game.GateIndex.Any(gate => gate.OnField && gate.Position.Y == (user.Position as GateCard).Position.Y && !gate.IsTouching(user.Position as GateCard));
    }

    internal class TunnelingEffect
    {
        public int TypeId { get; }
        Bakugan User;
        GateCard moveTarget;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public TunnelingEffect(Bakugan user, GateCard moveTarget, int typeID, bool IsCopy)
        {
            User = user;
            this.moveTarget = moveTarget;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            User.Move(moveTarget, new JObject() { ["MoveEffect"] = "Submerge" });
        }
    }
}
