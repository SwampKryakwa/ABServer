using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class HaosFreezeEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        Game game;

        GateCard target;

        public Player Owner { get => User.Owner; }

        public HaosFreezeEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
            target = game.Field.Cast<GateCard>().First(x => x.Bakugans.Contains(User));
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 11 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            target.Freeze(this);

            game.BakuganAdded += Trigger;

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganPowerReset += ResetTurnover;

            User.affectingEffects.Add(this);
        }

        public void Trigger(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (target.Position == pos)
            {
                this.target.TryUnfreeze(this);
                game.BakuganAdded -= Trigger;
            }
        }

        //remove when negated
        public void Negate()
        {
            if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganPowerReset -= ResetTurnover;

                target.TryUnfreeze(this);

                game.BakuganAdded -= Trigger;
            }
        }
        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;

            game.BakuganAdded -= Trigger;
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (leaver == User && User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class HaosFreeze : AbilityCard, IAbilityCard
    {
        public HaosFreeze(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new HaosFreezeEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.OnField() && user.Attribute == Attribute.Haos;

        public new int TypeId { get; private protected set; } = 11;
    }
}
