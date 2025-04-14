using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class AirBattle : AbilityCard
    {
        public AirBattle(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_ATTACKTARGET", TargetValidator = x => x.Owner.SideID != Owner.SideID && x.OnField() && IsAdjacent(x.Position, User.Position)}
            ];
        }

        public override void TriggerEffect() =>
            new AirBattleEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Zephyros)) && Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Zephyros) && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => x.OnField() && IsAdjacent(x.Position, user.Position) && user.IsEnemyOf(x));

        private static bool IsAdjacent(IBakuganContainer pos1, IBakuganContainer pos2)
        {
            if (pos1 is GateCard gate1 && pos2 is GateCard gate2)
                return GateCard.AreTouching(gate1, gate2);
            return false;
        }
    }
    internal class AirBattleEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public AirBattleEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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

            // Perform the attack
            if (target.Power < User.Power)
            {
                if (target.Position is GateCard positionGate)
                {
                    target.DestroyOnField(positionGate.EnterOrder);
                }
            }
        }
    }
}
