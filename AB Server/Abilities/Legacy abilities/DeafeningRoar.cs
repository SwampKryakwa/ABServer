using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class DeafeningRoarEffect
    {
        public int TypeId { get; }
        Bakugan user;
        Game game;
        List<Bakugan> affectedBakugan = new();

        public Player Owner { get => user.Owner; } bool IsCopy;

        public DeafeningRoarEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            this.user = user;
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
                    { "UserID", user.BID },
                    { "User", new JObject {
                        { "Type", (int)user.Type },
                        { "Attribute", (int)user.Attribute },
                        { "Tretment", (int)user.Treatment },
                        { "Power", user.Power }
                    }}
                });
            }

            foreach (Bakugan b in game.BakuganIndex.Where(x => x.Owner.SideID != Owner.SideID && x.OnField()))
            {
                b.Boost(new Boost(-100), this);
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

    internal class DeafeningRoar : AbilityCard
    {
        public DeafeningRoar(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new DeafeningRoarEffect(User, Game, TypeId, IsCopy).Activate();
            Dispose();
        }

        public new void DoubleEffect() =>
                new DeafeningRoarEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Griffon;
    }
}
