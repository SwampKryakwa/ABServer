using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class JudgementNightEffect : INegatable
    {
        public int TypeID { get; }
        Bakugan user;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return user.Owner;
        }

        public JudgementNightEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 0 },
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            game.Field.Cast<GateCard>().First(x => x.Bakugans.Contains(user)).DetermineWinner();
            if (!game.Field.Cast<GateCard>().Any(x => x.ActiveBattle))
            {
                game.isFightGoing = false;
                game.EndTurn();
            }
        }
        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter) counterNegated = true;
        }
    }

    internal class JudgementNight : AbilityCard, IAbilityCard
    {

        public JudgementNight(int cID, Player owner)
        {
            CID = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_boost_target" },
                { "Ability", 0 },
                { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Darkus && !x.UsedAbilityThisTurn).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new JudgementNightEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["bakugan"]], Game, 0);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return Game.BakuganIndex.Any(x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Darkus && !x.UsedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 0;
        }
    }
}
