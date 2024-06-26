﻿using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class BackfireEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        IGateCard target;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

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
            if (counterNegated) return;

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
        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }

    internal class Backfire : AbilityCard, IAbilityCard
    {
        public Backfire(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
            BakuganIsValid = x => x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Pyrus && !x.UsedAbilityThisTurn && Game.GateIndex.Any(x => x.OnField && x.IsOpen);
        }

        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 2 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", 2 },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
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
                        { "Ability", 2 },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen && x.OnField).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y }
                        })) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user, Action finishOriginal)
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "gate_negate_target" },
                        { "Ability", 2 },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen && x.OnField).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y }
                        })) }
                    } }
                }
            });

            Game.awaitingAnswers[Owner.ID] = () => ResolveFusion(user, finishOriginal);
        }

        public new void Resolve()
        {
            Bakugan user = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];
            var effect = new BackfireEffect(user, Game.GateIndex[(int)Game.IncomingSelection[Owner.ID]["array"][1]["gate"]], Game, 1);

            Game.SuggestFusion(Owner, this, user, () =>
            {
                effect.Activate();
                Dispose();
            });
        }

        public void ResolveFusion(Bakugan user, Action finishOriginal)
        {
            var effect = new BackfireEffect(user, Game.GateIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["gate"]], Game, 1);

            //window for counter

            finishOriginal();
            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter() => IsActivateable(false);

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable(bool asFusion) => IsActivateable(false);

        public new int GetTypeID() => 2;
    }
}
