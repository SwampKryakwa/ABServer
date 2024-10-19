using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DarkonGravityEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; }

        public DarkonGravityEffect(Bakugan user, Game game, int typeID)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
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

            Bakugan.MultiMove(game, User.Position as GateCard, game.BakuganIndex.Where(x => x != User && x.OnField()).ToArray());
        }
    }

    internal class DarkonGravity : AbilityCard, IAbilityCard
    {
        public DarkonGravity(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new DarkonGravityEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new DarkonGravityEffect(User, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Centipede && user.Attribute == Attribute.Darkon && user.Owner.Bakugans.Count == 0;


    }
}
