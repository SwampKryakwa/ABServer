using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class GrandDown : AbilityCard
    {
        public GrandDown(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = owner.Id, Message = "INFO_ABILITY_GATENEGATETARGET", TargetValidator = x => x.OnField && x.IsOpen}
            ];
        }

        public override void TriggerEffect() =>
                new GrandDownEffect(User, (TargetSelectors[0] as GateSelector).SelectedGate, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Attribute == Attribute.Darkon && Game.GateIndex.Any(x => x.OnField && x.IsOpen);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.GateIndex.Any(x => x.OnField && x.IsOpen);
    }
    internal class GrandDownEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        GateCard target;
        Game game { get => User.Game; }

        public Player Onwer { get; set; }
        bool IsCopy;

        public GrandDownEffect(Bakugan user, GateCard target, int typeID, bool IsCopy)
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
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            target.Negate();
        }
    }
}
