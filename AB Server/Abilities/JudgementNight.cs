using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class JudgementNightEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;


        public Player Owner { get => User.Owner; }

        public JudgementNightEffect(Bakugan user, Game game, int typeID)
        {
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
                    { "Card", 0 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            game.Field.Cast<GateCard>().First(x => x.Bakugans.Contains(User)).DetermineWinner();
            if (!game.Field.Cast<GateCard>().Any(x => x.ActiveBattle))
            {
                game.isBattleGoing = false;
                game.EndTurn();
            }
        }
        //remove when negated
        public void Negate()
        {

        }
    }

    internal class JudgementNight : AbilityCard, IAbilityCard
    {

        public JudgementNight(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new JudgementNightEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Darkon;
    }
}
