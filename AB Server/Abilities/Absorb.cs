using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class AbsorbEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;
        short boost;


        public Player Owner { get => User.Owner; }

        public AbsorbEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            target = target;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            boost = (short)((game.BakuganIndex.Count(x => !x.OnField() && !x.InHands) + 1) * 50);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 17 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            User.Boost(boost, this);

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;

            User.affectingEffects.Add(this);
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class Absorb : AbilityCard, IAbilityCard
    {
        public Absorb(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new AbsorbEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]], Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Aqua;

        public new int TypeId { get; private protected set; } = 17;
    }
}
