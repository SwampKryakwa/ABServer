using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DustVeilEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public DustVeilEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
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

            foreach (var bakugan in game.BakuganIndex)
            {
                if (bakugan.Owner == Owner)
                    bakugan.Boost(new Boost(50), this);
                else if (bakugan.Owner.SideID != Owner.SideID)
                    bakugan.Boost(new Boost(-50), this);
            }
        }
    }

    internal class PowderVeil : AbilityCard
    {
        public PowderVeil(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new DustVeilEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new DustVeilEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Fairy;


    }
}
