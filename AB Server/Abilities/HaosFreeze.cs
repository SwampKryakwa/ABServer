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
            user.UsedAbilityThisTurn = true;
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
            target.Freeze(this);

            game.BakuganAdded += Trigger;

            game.NegatableAbilities.Add(this);
            game.TurnEnd += NegatabilityTurnover;

            game.BakuganPowerReset += ResetTurnover;

            User.affectingEffects.Add(this);
        }

        public void Trigger(Bakugan target, ushort owner, BakuganContainer pos)
        {
            if (target.Position == pos)
            {
                this.target.TryUnfreeze(this);
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

                target.TryUnfreeze(this);

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
            if (leaver == User && User.affectingEffects.Contains(this))
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
            Owner = owner;
            Game = owner.game;
        }
        public new void Activate()
        {
            Game.NewEvents[Owner.ID].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "SelectionType", "B" },
                { "Message", "ability_boost_target" },
                { "Ability", 11 },
                { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(x => x.InBattle && x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Haos && !x.UsedAbilityThisTurn).Select(x =>
                    new JObject { { "Type", (int)x.Type },
                        { "Attribute", (int)x.Attribute },
                        { "Treatment", (int)x.Treatment },
                        { "Power", x.Power },
                        { "Owner", x.Owner.ID },
                        { "BID", x.BID }
                    }
                )) }
            });

            Game.awaitingAnswers[Owner.ID] = Resolve;
        }

        public new void Resolve()
        {
            var effect = new ShiningBrillianceEffect(Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["bakugan"]], Game, 0);

            //window for counter

            effect.Activate();
            Dispose();
        }

        public new void ActivateCounter()
        {
            Activate();
        }

        public new void ActivateFusion(IAbilityCard fusedWith, Bakugan user)
        {
            Activate();
        }

        public new bool IsActivateable()
        {
            return Game.BakuganIndex.Any(x => x.InBattle && x.OnField() && x.Owner == Owner && x.Attribute == Attribute.Haos && !x.UsedAbilityThisTurn);
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
