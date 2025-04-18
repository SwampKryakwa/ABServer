using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class Dimension4 : AbilityCard
    {
        public Dimension4(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Position == User.Position}
            ];
        }

        public override void TriggerEffect() =>
            new Dimension4Effect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.Type == BakuganType.Lucifer && user.InBattle && user.Position.Bakugans.Any(x => x.IsEnemyOf(user));

        public static new bool HasValidTargets(Bakugan user) =>
            user.Type == BakuganType.Lucifer && user.OnField();
    }

    internal class Dimension4Effect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public Dimension4Effect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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

            // Set the power of the target Bakugan to its initial value
            target.Boost(new Boost((short)(target.DefaultPower - target.Power)), this);
        }
    }
}


