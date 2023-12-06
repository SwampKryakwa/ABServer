using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace AB_Server.Abilities
{
    internal class HaosFreezeEffect : INegatable
    {
        public int TypeID { get; }
        public Bakugan User;
        Game game;
        bool counterNegated = false;
        GateCard target;

        public Player GetOwner()
        {
            return User.Owner;
        }

        public HaosFreezeEffect(Bakugan user, Game game, int typeID)
        {
            User = user;
            this.game = game;
            user.usedAbilityThisTurn = true;
            TypeID = typeID;
            target = game.Field.Cast<GateCard>().First(x => x.Bakugans.Contains(User));
        }

        public void Activate()
        {
            int team = User.Owner.SideID;

            for (int i = 0; i < game.NewEvents.Length; i++)
            {
                game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityActivateEffect" },
                    { "Card", 11 },
                    { "UserID", User.BID },
                    { "User", new JObject {
                        { "Type", (int)User.Type },
                        { "Attribute", (int)User.Attribute },
                        { "Tretment", (int)User.Treatment },
                        { "Power", User.Power }
                    }}
                });
            }
            target.Freeze();

            game.BakuganAdded += Trigger;

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganPowerReset += ResetTurnover;

            User.affectingEffects.Add(this);
        }

        public void Trigger(Bakugan target, ushort owner, int pos)
        {
            if (target.Position == pos)
            {
                this.target.IsFrozen = false;
                game.isFightGoing |= this.target.CheckBattles();
                game.BakuganAdded -= Trigger;
            }
        }

        //remove when negated
        public void Negate(bool asCounter)
        {
            if (asCounter) counterNegated = true;
            else if (User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganPowerReset -= ResetTurnover;

                target.IsFrozen = false;
                game.isFightGoing |= target.CheckBattles();

                game.BakuganAdded -= Trigger;
            }
        }
        //is not negatable after turn ends
        public void NegatabilityTurnover()
        {
            game.NegatableAbilities.Remove(this);
            game.TurnEnd -= NegatabilityTurnover;

            game.BakuganAdded -= Trigger;
        }

        //remove when power reset
        public void ResetTurnover(Bakugan leaver)
        {
            if (leaver == User & User.affectingEffects.Contains(this))
            {
                User.affectingEffects.Remove(this);
                game.BakuganPowerReset -= ResetTurnover;
            }
        }
    }

    internal class HaosFreeze : AbilityCard, IAbilityCard
    {
        public HaosFreeze(int cID, Player owner)
        {
            CID = cID;
            this.owner = owner;
            game = owner.game;
        }
        public new void Activate()
        {
            game.NewEvents[owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_boost_target" },
                { "Ability", 11 },
                { "SelectionBakugans", new JArray(game.BakuganIndex.Where(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Haos & !x.usedAbilityThisTurn).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });

            game.awaitingAnswers[owner.ID] = Resolve;
        }

        public void Resolve()
        {
            var effect = new ShiningBrillianceEffect(game.BakuganIndex[(int)game.IncomingSelection[owner.ID]["bakugan"]], game, 0);

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
            return game.BakuganIndex.Any(x => x.InBattle & x.Position >= 0 & x.Owner == owner & x.Attribute == Attribute.Haos & !x.usedAbilityThisTurn);
        }

        public new bool IsActivateable(bool asFusion)
        {
            return IsActivateable();
        }

        public new int GetTypeID()
        {
            return 11;
        }
    }
}
