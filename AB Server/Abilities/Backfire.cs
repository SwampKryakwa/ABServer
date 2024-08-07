﻿using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class BackfireEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        IGateCard target;
        Game game;


        public Player Owner { get => User.Owner; }

        public BackfireEffect(Bakugan user, IGateCard target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
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
                    { "Card", 2 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            target.Negate();
        }

        //remove when negated
        public void Negate() { }
    }

    internal class Backfire : AbilityCard, IAbilityCard, INegatable
    {
        public Backfire(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        private IGateCard target;

        public void Setup()
        {
            IAbilityCard ability = this;
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(ability.BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "gate_negate_target" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen && x.OnField).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y }
                        })) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public new void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "gate_negate_target" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen && x.OnField).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y }
                        })) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = ActivateFusion;
        }

        public new void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];
            target = Game.GateIndex[(int)Game.IncomingSelection[Owner.ID]["array"][1]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public void ActivateFusion()
        {
            target = Game.GateIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public void Negate() =>
            counterNegated = true;

        public new void Resolve()
        {
            if (!counterNegated)
                new BackfireEffect(User, target, Game, TypeId).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) => user.OnField() && user.Attribute == Attribute.Pyrus && Game.GateIndex.Any(x => x.OnField && x.IsOpen);

        public new int TypeId { get; } = 2;
    }
}
