﻿using AB_Server.Gates;
using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class MagmaSurfaceEffect : INegatable
    {
        public int TypeId { get; }
        public Bakugan User;
        IGateCard target;
        IGateCard replacement;
        Game game;


        public Player Owner { get => User.Owner; }

        public MagmaSurfaceEffect(Bakugan user, IGateCard target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.UsedAbilityThisTurn = true;
            TypeId = typeID;
        }

        public void Activate()
        {


            target.Negate();

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 6 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

            replacement = new NormalGate(Attribute.Subterra, 50, target.CardId, game, target.Owner);
            game.Field[target.Position.X, target.Position.Y] = replacement as GateCard;
            game.GateIndex[target.CardId] = replacement;

            replacement.DisallowedPlayers = target.DisallowedPlayers;
            replacement.Position = target.Position;
            replacement.Bakugans = target.Bakugans;
            replacement.ActiveBattle = target.ActiveBattle;

            foreach (var e in game.NewEvents)
            {
                JObject obj = new()
                {
                    { "Type", "GateTypeChange" },
                    { "PosX", target.Position.X }, { "PosY", target.Position.Y },
                    { "GateData", new JObject {
                        { "Type", 0 },
                        { "Attribute", 5 },
                        { "Power", 100 }
                    }},
                    { "Owner", target.Owner.ID },
                    { "CID", target.CardId }
                };
            }

            replacement.Open();

            game.NegatableAbilities.Add(this);
        }

        public void Restore()
        {
            target.DisallowedPlayers = replacement.DisallowedPlayers;
            target.Position = replacement.Position;
            target.Bakugans = replacement.Bakugans;
            target.ActiveBattle = replacement.ActiveBattle;

            if (replacement.Owner.GateGrave.Contains(replacement))
            {
                replacement.Owner.GateGrave.Remove(replacement);
                replacement.Owner.GateGrave.Add(target);
            }
            if (replacement.Owner.GateHand.Contains(replacement))
            {
                replacement.Owner.GateHand.Remove(replacement);
                replacement.Owner.GateHand.Add(target);
            }
            if (game.Field.Cast<GateCard>().Contains(replacement))
            {
                game.Field[replacement.Position.X, replacement.Position.Y] = target as GateCard;
            }
            game.GateIndex[replacement.CardId] = target;
        }

        //remove when negated
        public void Negate()
        {
            Restore();
        }
    }

    internal class MagmaProminence : AbilityCard, IAbilityCard
    {
        public MagmaProminence(int cID, Player owner)
        {
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public new void Setup(bool asCounter)
        {
            IAbilityCard ability = this;
            Game.AbilityChain.Add(this);
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 2 },
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
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "INFO_GATENEGATETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X }, { "PosY", x.Position.Y }
                        })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "G" },
                        { "Message", "INFO_GATENEGATETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => x.IsOpen).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X }, { "PosY", x.Position.Y }
                        })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = ActivateFusion;
        }

        private IGateCard target;

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

        public new void Resolve()
        {
            if (!counterNegated)
                new MagmaSurfaceEffect(User, target, Game, 1).Activate();

            Dispose();
        }

        public new bool IsActivateableFusion(Bakugan user) =>
            user.OnField() && user.Attribute == Attribute.Subterra && Game.GateIndex.Any(y => y.IsOpen);

        public new int TypeId { get; } = 6;
    }
}
