﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BurstReturnEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public BurstReturnEffect(Bakugan user, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
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

            User.Revive();

            foreach (var bakugan in game.BakuganIndex.Where(x => x.OnField()))
                bakugan.Boost(new Boost(-50), this);
        }
    }

    internal class BurstReturn : AbilityCard
    {
        public BurstReturn(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            var ability = this as AbilityCard;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BH" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = ability.Activate;
        }

        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new BurstReturnEffect(User, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new BurstReturnEffect(User, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Raptor && user.InGrave();
    }
}
