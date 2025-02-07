using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class FireWallEffect : IActive
    {
        public int TypeId { get; }
        public int EffectId { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Effect;
        Bakugan User;
        Game game;
        Dictionary<Bakugan, Boost> AffectedBakugan = new();

        public Player Owner { get => User.Owner; } bool IsCopy;

        public FireWallEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
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

            foreach (var bakugan in game.BakuganIndex.Where(bakugan => bakugan.Owner.SideID != Owner.SideID))
            {
                Boost boost = new(-50);
                AffectedBakugan.Add(bakugan, boost);
                bakugan.Boost(boost, this);
            }

            (User.Position as GateCard).MovingAwayEffectBlocking.Add(this);
            (User.Position as GateCard).MovingInEffectBlocking.Add(this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;
        }

        private void OnBakuganLeaveField(Bakugan target, ushort owner)
        {
            if (AffectedBakugan.Keys.Contains(target))
            {
                Boost boost = new(-50);
                AffectedBakugan[target] = boost;
                User.Boost(boost, this);
            }
            else if (target == User)
            {
                Negate(false);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

            foreach (var bakugan in AffectedBakugan.Keys)
            {
                AffectedBakugan[bakugan].Active = false;
                bakugan.RemoveBoost(AffectedBakugan[bakugan], this);
            }

            (User.Position as GateCard).MovingAwayEffectBlocking.Remove(this);
            (User.Position as GateCard).MovingInEffectBlocking.Remove(this);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "EffectRemovedActiveZone" },
                    { "Id", EffectId }
                });
            }
        }
    }

    internal class FireWall : AbilityCard
    {
        public FireWall(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new FireWallEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new FireWallEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Nova;
    }
}
