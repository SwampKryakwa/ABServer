using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Blowback : AbilityCard
    {
        public Blowback(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_RETRACTTARGET", TargetValidator = target => IsTargetValid(target, User)}
            ];
        }

        public override void TriggerEffect() =>
            new BlowbackEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.IsAttribute(Attribute.Zephyros) && user.OnField() && Game.CurrentWindow == ActivationWindow.Normal;

        public static bool IsTargetValid(Bakugan target, Bakugan user) =>
            target.OnField() && target.Owner == user.Owner;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(target => IsTargetValid(target, user));
    }

    internal class BlowbackEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public BlowbackEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
        {
            User = user;
            this.target = target;
            this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" }, { "Kind", 0 },
                    { "Card", TypeId },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.MainAttribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            if (target.Position is GateCard positionGate)
                target.ToHand(positionGate.EnterOrder);
        }
    }
}
