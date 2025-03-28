﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class MoltenCoreEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;
        bool hasFusions;
        bool isFusion;
        AbilityCard fusedTo;

        public Player Owner { get => User.Owner; } bool IsCopy;

        public MoltenCoreEffect(Bakugan user, bool hasFusions, bool isFusion, AbilityCard fusedTo, Game game, int typeID, bool IsCopy)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
            this.game = game;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
            TypeId = typeID;
            this.hasFusions = hasFusions;
            this.isFusion = isFusion;
            this.fusedTo = fusedTo;
        }

        public void Activate()
        {
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

            if (hasFusions)
                User.Boost(new Boost(100), this);

            if (isFusion)
                fusedTo.DoubleEffect();
        }
    }

    internal class MoltenCore : AbilityCard
    {
        public MoltenCore(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new MoltenCoreEffect(User, Fusion != null, FusedTo != null, FusedTo, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new MoltenCoreEffect(User, Fusion != null, FusedTo != null, FusedTo, Game, TypeId, IsCopy).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.InBattle && !user.Owner.BakuganOwned.Any(x => x.Attribute != Attribute.Nova);
    }
}
