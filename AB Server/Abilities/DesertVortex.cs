using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class DesertVortexEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        IGateCard target;
        Game game;
        bool counterNegated = false;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public DesertVortexEffect(Bakugan user, IGateCard target, Game game, int typeID)
        {
            User = user;
            this.game = game;
            this.target = target;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
        }

        public void Activate()
        {
            if (counterNegated) return;

            User.Move(target.Position);

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 7 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }

        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }

    internal class DesertVortex : AbilityCard, IAbilityCard
    {
        public DesertVortex(int cID, Player owner)
        {
            CID = cID;
            this.owner = owner;
            game = owner.game;
        }

        public new void Activate()
        {
            game.NewEvents[owner.ID].Add(new JObject
            {
                { "Type", "StartSelectionArr" },
                { "Count", 7 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "B" },
                        { "Message", "ability_user" },
                        { "Ability", 7 },
                        { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => game.Field.Cast<GateCard>().Any(y => y?.Owner != owner && y.Bakugans?.Any(z => z.Owner.SideID != x.Owner.SideID) == true && y?.IsTouching(x.Position) == true) & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Subterra & !x.usedAbilityThisTurn).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID }
                            }
                        )) } },
                    new JObject {
                        { "SelectionType", "G?" },
                        { "Message", "gate_move_target" },
                        { "Ability", 7 },
                        { "SelectionRange", "TGHE" },
                        { "CompareTo", 0 }
                    } }
                }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new DesertVortexEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["array"][0]["bakugan"]], game.Field[(int)game.IncomingSelection[owner.ID]["array"][1]["pos"] / 10, (int)game.IncomingSelection[owner.ID]["array"][1]["pos"] % 10], game, 1);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion()
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return game.BakuganIndex.Any(x => game.Field.Cast<GateCard>().Any(y => y?.Owner != owner && y.Bakugans?.Any(z => z.Owner.SideID != x.Owner.SideID) == true && y?.IsTouching(x.Position) == true) & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Subterra & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 7;
        }
    }
}
