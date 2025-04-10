using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Correlations
{
    internal class DiagonalCorrelation : AbilityCard
    {
        public DiagonalCorrelation(int cID, Player owner) : base(cID, owner, 1)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BF", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => x.Owner == User.Owner && Bakugan.IsDiagonal(x, User)}
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect() =>
            new DiagonalCorrelationEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => Bakugan.IsDiagonal(x, user) && x.Owner == user.Owner && x.OnField());
    }

    internal class DiagonalCorrelationEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public DiagonalCorrelationEffect(Bakugan user, Bakugan target, int typeID, bool IsCopy)
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
                    { "Kind", 2 },
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

            User.Boost(new Boost(100), this);
            target.Boost(new Boost(100), this);
        }

    }
}