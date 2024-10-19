﻿using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class ShadeAbilityEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        IAbilityCard target;
        bool isCounter;
        Game game;

        public Player Owner { get => User.Owner; }

        public ShadeAbilityEffect(Bakugan user, IAbilityCard target, bool isCounter, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            this.isCounter = isCounter;
            TypeId = typeID;

            user.UsedAbilityThisTurn = true;
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

            target.Negate(isCounter);
        }
    }

    internal class ShadeAbility : AbilityCard, IAbilityCard
    {

        public ShadeAbility(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        private IAbilityCard target;
        private bool isCounter = false;

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            isCounter = asCounter;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
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

            Game.awaitingAnswers[Owner.Id] = Setup2;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.ToArray())
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    EventBuilder.ActiveSelection("INFO_ABILITYNEGATETARGET", Game.ActiveZone.ToArray())
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void Activate()
        {
            target = Game.AbilityIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["ability"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new ShadeAbilityEffect(User, target, isCounter, Game, TypeId).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
                new ShadeAbilityEffect(User, target, isCounter, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) => user.OnField() && !user.Owner.BakuganOwned.Any(x => x.Attribute != Attribute.Lumina);
    }
}
