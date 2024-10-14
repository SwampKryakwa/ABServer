using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class OreganoMurderEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;
        List<Bakugan> affectedBakugan = new();

        public Player Owner { get => user.Owner; }

        public OreganoMurderEffect(Bakugan user, Game game, int typeID)
        {
            this.user = user;
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
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }
            user.Boost(100, this);
            affectedBakugan.Add(user);
            user.affectingEffects.Add(this);

            foreach (Bakugan b in user.Position.Bakugans.Where(x => x.Owner.SideID != Owner.SideID))
            {
                b.Boost(-100, this);
                affectedBakugan.Add(b);
                b.affectingEffects.Add(this);
            }

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;

            user.affectingEffects.Add(this);
        }

        //remove when goes to hand
        //remove when goes to grave
        public void FieldLeaveTurnover(Bakugan leaver, ushort owner)
        {
            if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                affectedBakugan.Remove(leaver);
                if (affectedBakugan.Count == 0)
                {
                    game.BakuganReturned -= FieldLeaveTurnover;
                    game.BakuganDestroyed -= FieldLeaveTurnover;
                    game.BakuganPowerReset -= ResetTurnover;
                }
            }
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (user.affectingEffects.Contains(this))
            {
                user.affectingEffects.Remove(this);
                affectedBakugan.Remove(leaver);
                if (affectedBakugan.Count == 0)
                {
                    game.BakuganReturned -= FieldLeaveTurnover;
                    game.BakuganDestroyed -= FieldLeaveTurnover;
                    game.BakuganPowerReset -= ResetTurnover;
                }
            }
        }
    }

    internal class OreganoMurder : AbilityCard, IAbilityCard, INegatable
    {
        public new int TypeId { get; private protected set; }

        public OreganoMurder(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new OreganoMurderEffect(User, Game, TypeId).Activate();
            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.Attribute == Attribute.Darkon && (user.Owner.BakuganOwned.Count(x => x == user || x.InGrave()) == user.Owner.BakuganOwned.Count);
    }
}
