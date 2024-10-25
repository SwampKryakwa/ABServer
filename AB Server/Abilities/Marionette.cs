using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks.Dataflow;

namespace AB_Server.Abilities
{
    internal class MarionetteEffect
    {
        public int TypeId { get; }
        Bakugan User;
        Bakugan target;
        GateCard moveTarget;
        Game game;

        public Player Owner { get => User.Owner; }

        public MarionetteEffect(Bakugan user, Bakugan target, GateCard moveTarget, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            this.moveTarget = moveTarget;
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

            target.Move(moveTarget);
        }
    }

    internal class Marionette : AbilityCard, IAbilityCard
    {
        public Marionette(int cID, Player owner, int typeId)
        {
            TypeId = typeId;
            CardId = cID;
            Owner = owner;
            Game = owner.game;
        }

        public void Setup(bool asFusion)
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

            Game.awaitingAnswers[Owner.Id] = Setup2;
        }

        public void Setup2()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.OnField() && x.Owner.SideID != Owner.SideID && x.Position != User.Position).Select(x =>
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

            Game.awaitingAnswers[Owner.Id] = Setup3;
        }

        Bakugan target;

        public void Setup3()
        {
            target = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "GF" },
                        { "Message", "INFO_MOVETARGET" },
                        { "Ability", TypeId },
                        { "SelectionGates", new JArray(Game.GateIndex.Where(x => (target.Position as GateCard).IsTouching(x as GateCard)).Select(x => new JObject {
                            { "Type", x.TypeId },
                            { "PosX", x.Position.X },
                            { "PosY", x.Position.Y },
                            { "CID", x.CardId }
                        })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;
            parentCard.Fusion = this;

            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYTARGET" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x=>x.OnField() && x.Owner.SideID != Owner.SideID && x.Position != User.Position).Select(x =>
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

            Game.awaitingAnswers[Owner.Id] = Setup3;
        }

        IGateCard moveTarget;

        public void Activate()
        {
            moveTarget = Game.GateIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["gate"]];

            Game.CheckChain(Owner, this, User);
        }

        public new void Resolve()
        {
            if (!counterNegated)
                new MarionetteEffect(User, target, moveTarget as GateCard, Game, TypeId).Activate();
            Dispose();
        }

        public new void DoubleEffect() =>
                new MarionetteEffect(User, target, moveTarget as GateCard, Game, TypeId).Activate();

        public bool IsActivateableFusion(Bakugan user) =>
            user.Type == BakuganType.Mantis && user.OnField();


    }
}
