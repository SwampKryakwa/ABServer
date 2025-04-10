using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities.Correlations
{
    internal class AdjacentCorrelation : AbilityCard
    {

        public AdjacentCorrelation(int cID, Player owner) : base(cID, owner, 0)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "B", ForPlayer = owner.Id, Message = "INFO_ABILITY_TARGET", TargetValidator = x => ((x.InHand() && x.Owner == User.Owner) || x.OnField()) && Bakugan.IsAdjacent(User, x)}
            ];
        }

        public override CardKind Kind { get; } = CardKind.CorrelationAbility;

        public override void TriggerEffect() =>
            new AdjacentCorrelationEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && HasValidTargets(user);

        public static new bool HasValidTargets(Bakugan user) =>
            user.Game.BakuganIndex.Any(x => Bakugan.IsAdjacent(user, x) && (x.OnField() || (x.Owner == user.Owner && x.InHand())));
    }

    internal class AdjacentCorrelationEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy;

        public AdjacentCorrelationEffect(Bakugan user, int typeID, bool IsCopy)
        {
            User = user;
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
        }
    }
}