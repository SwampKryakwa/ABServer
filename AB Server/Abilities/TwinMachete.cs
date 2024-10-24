﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class TwinMacheteEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Game game;

        public Player Owner { get => User.Owner; }

        public TwinMacheteEffect(Bakugan user, Game game, int typeID)
        {
            Console.WriteLine(typeof(FireJudgeEffect));
            User = user;
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
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            User.Boost(new Boost(100), this);
        }
    }

    internal class TwinMachete : AbilityCard, IAbilityCard
    {
        public TwinMachete(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new TwinMacheteEffect(User, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new TwinMacheteEffect(User, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Type == BakuganType.Mantis;

        
    }
}
