using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TornadoWallEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;


        public Player Owner { get => User.Owner; }

        public TornadoWallEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            target = target;
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
                    { "Card", 18 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            foreach (Bakugan b in game.BakuganIndex.Where(x => x.OnField()))
            {
                if (b.Owner == User.Owner && b.Attribute == Attribute.Ventus)
                {
                    b.Boost(80, this);
                    b.affectingEffects.Add(this);
                    break;
                }
                if (b.Owner.SideID != User.Owner.SideID)
                {
                    b.Boost(-80, this);
                    b.affectingEffects.Add(this);
                }
            }

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganReturned += FieldLeaveTurnover;
            game.BakuganDestroyed += FieldLeaveTurnover;
            game.BakuganPowerReset += ResetTurnover;
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

        //remove when negated
        public void Negate()
        {
            if (User.affectingEffects.Contains(this))
            {
                target.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
                foreach (Bakugan b in game.BakuganIndex)
                {
                    if (b.Owner == User.Owner && b.affectingEffects.Contains(this))
                    {
                        b.Boost(-80, this);
                        b.affectingEffects.Remove(this);
                        return;
                    }
                    if (b.Owner.SideID != User.Owner.SideID && b.affectingEffects.Contains(this))
                    {
                        b.Boost(80, this);
                        b.affectingEffects.Remove(this);
                    }
                }
            }
        }
        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (leaver.affectingEffects.Contains(this))
            {
                leaver.affectingEffects.Remove(this);
                game.BakuganReturned -= FieldLeaveTurnover;
                game.BakuganDestroyed -= FieldLeaveTurnover;
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class TornadoWall : AbilityCard, IAbilityCard
    {
        public TornadoWall(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new TornadoWallEffect(User, Game, 1).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            !user.InGrave() && user.Attribute == Attribute.Ventus;

        public new int TypeId { get; } = 18;
    }
}
