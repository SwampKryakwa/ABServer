using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class WaterRefrainEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        int turnsPassed = 0;

        public Player Owner { get => User.Owner; }

        public WaterRefrainEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

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
            game.Players.ForEach(p => p.AbilityBlockers.Add(this));

            game.TurnEnd += CheckEffectOver;

            User.affectingEffects.Add(this);
        }

        //is not negatable after turn ends
        public void CheckEffectOver()
        {
            if (turnsPassed++ == 1)
            {
                game.Players.ForEach(x => { if (x.AbilityBlockers.Contains(this)) x.AbilityBlockers.Remove(this); });
                game.TurnEnd -= CheckEffectOver;
            }
        }
    }

    internal class WaterRefrain : AbilityCard, IAbilityCard
    {
        public WaterRefrain(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new WaterRefrainEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Attribute == Attribute.Aqua && user.OnField();

        public new int TypeId { get; private protected set; } = 0;
    }
}
