using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class CinderCoilEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        Bakugan User;
        Game game;
        List<Boost> affectedBoosts = new();

        public Player Owner { get => User.Owner; } bool IsCopy;

        public CinderCoilEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
            EffectId = game.NextEffectId++;
        }

        public void Activate()
        {
            game.ActiveZone.Add(this);

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
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectAddedActiveZone" }, { "IsCopy", IsCopy },
                    { "Card", TypeId },
                    { "Id", EffectId },
                    { "Owner", Owner.Id }
                });
            }

            game.BakuganBoosted += Trigger;
        }

        public void Trigger(Bakugan target, Boost boost, object source)
        {
            if (boost.Value > 0 && target == User && source.GetType() != typeof(CinderCoilEffect))
            {
                boost.Value += 400;
                affectedBoosts.Add(boost);
                foreach (var e in game.NewEvents)
                {
                    e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", Owner.Id },
                        { "Boost", 400 },
                        { "Bakugan", new JObject {
                            { "Type", (int)target.Type },
                            { "Attribute", (int)target.Attribute },
                            { "Treatment", (int)target.Treatment },
                            { "Power", target.Power },
                            { "BID", target.BID } }
                        }
                    });
                }
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);
            game.BakuganBoosted -= Trigger;
            int reduction = 0;

            foreach (var boost in affectedBoosts.Where(x => x.Active))
            {
                boost.Value -= 400;
                reduction += 400;
            }

            foreach (var e in game.NewEvents)
            {
                e.Add(new JObject {
                        { "Type", "BakuganBoostedEvent" },
                        { "Owner", Owner.Id },
                        { "Boost", -reduction },
                        { "Bakugan", new JObject {
                            { "Type", (int)User.Type },
                            { "Attribute", (int)User.Attribute },
                            { "Treatment", (int)User.Treatment },
                            { "Power", User.Power },
                            { "BID", User.BID } }
                        }
                    });
            }
        }
    }

    internal class CinderCoil : AbilityCard, IAbilityCard
    {

        public CinderCoil(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new CinderCoilEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new CinderCoilEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Serpent && user.Attribute == Attribute.Nova;
    }
}