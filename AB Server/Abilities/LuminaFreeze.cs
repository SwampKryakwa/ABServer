using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class LuminaFreezeEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        public Bakugan User;
        Game game;

        GateCard target;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public LuminaFreezeEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
            target = user.Position as GateCard;
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
            target.Freeze(this);

            game.TurnEnd += Trigger;
        }

        public void Trigger()
        {
            if (game.TurnPlayer == Owner.Id)
            {
                target.TryUnfreeze(this);

                game.TurnEnd -= Trigger;
            }
        }

        public void Negate(bool asCounter)
        {
            target.TryUnfreeze(this);

            game.TurnEnd += Trigger;
        }
    }

    internal class LuminaFreeze : AbilityCard, IAbilityCard
    {
        public LuminaFreeze(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new LuminaFreezeEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new LuminaFreezeEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && user.Attribute == Attribute.Lumina;
    }
}
