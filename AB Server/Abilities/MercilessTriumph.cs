using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class MercilessTriumph : AbilityCard
    {
        public MercilessTriumph(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = target => IsTargetValid(target, User)}
            ];
        }

        public override void TriggerEffect() =>
            new MercilessTriumphEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.BattleEnd && user.Type == BakuganType.Glorius && user.OnField() && user.JustEndedBattle && !user.BattleEndedInDraw && Game.BakuganIndex.Any(target => IsTargetValid(target, user));

        public static bool IsTargetValid(Bakugan target, Bakugan user) =>
            target.OnField() && target != user;

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(target => IsTargetValid(target, user));
    }

    internal class MercilessTriumphEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public MercilessTriumphEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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
                        { "Treatment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            short powerReduction = (short)(-target.Power);
            target.Boost(new Boost(powerReduction), this);
        }
    }
}




