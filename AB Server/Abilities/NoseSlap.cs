﻿using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class NoseSlapEffect
    {
        public int TypeId { get; }
        public Bakugan User;
        Bakugan target;
        Game game;


        public Player Owner { get => User.Owner; }
        bool IsCopy;

        public NoseSlapEffect(Bakugan user, Bakugan target, Game game, int typeID, bool IsCopy)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true; this.IsCopy = IsCopy;
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

            if (target.Power < User.Power)
                target.Destroy((target.Position as GateCard).EnterOrder);
        }
    }

    internal class NoseSlap : AbilityCard, IAbilityCard
    {
        public NoseSlap(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asCounter)
        {
            IAbilityCard ability = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
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
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];
            IAbilityCard ability = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ValidTarget).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            if (parentCard != null) parentCard.Fusion = this;

            user.Game.NewEvents[user.Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(user.Game.BakuganIndex.Where(ValidTarget).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID }
                            }
                        )) }
                    }
                } }
            });

            user.Game.AwaitingAnswers[user.Owner.Id] = Activate;
        }

        private Bakugan target;

        public void Activate()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new NoseSlapEffect(User, target, Game, TypeId, IsCopy).Activate();

            Dispose();
        }

        public new void DoubleEffect() =>
            new NoseSlapEffect(User, target, Game, TypeId, IsCopy).Activate();

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
            if (target == bakugan)
                target = Bakugan.GetDummy();
        }

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Elephant && user.OnField() && user.Game.BakuganIndex.Any(ValidTarget);

        public bool ValidTarget(Bakugan target) =>
            target.Owner.SideID != Owner.SideID && target.OnField() && (target.Position as GateCard).IsTouching(User.Position as GateCard);

        public static bool HasValidTargets(Bakugan user) =>
            user.Position is GateCard && user.Game.BakuganIndex.Any(target => target.Owner.SideID != user.Owner.SideID && target.OnField() && (target.Position as GateCard).IsTouching(user.Position as GateCard));
    }
}
