using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    public enum AbilityKind
    {
        NormalAbility,
        FusionAbility,
        AlignmentAbility,
        Gate
    }

    internal class AbilityCard : IActive, IChainable
    {
        public static (Func<int, Player, AbilityCard> constructor, Func<Bakugan, bool> validTarget)[] AbilityCtrs =
        [
            //Set 1 attribute abilities
            ((cID, owner) => new FireJudge(cID, owner, 0), FireJudge.HasValidTargets),
            ((cID, owner) => new SpiritCanyon(cID, owner, 1), SpiritCanyon.HasValidTargets),
            ((cID, owner) => new HolyLight(cID, owner, 2), HolyLight.HasValidTargets),
            ((cID, owner) => new GrandDown(cID, owner, 3), GrandDown.HasValidTargets),
            ((cID, owner) => new WaterRefrain(cID, owner, 4), WaterRefrain.HasValidTargets),
            ((cID, owner) => new Blowback(cID, owner, 5), Blowback.HasValidTargets),

            //Set 1 Bakugan abilities
            ((cID, owner) => new Marionette(cID, owner, 6), Marionette.HasValidTargets),
            ((cID, owner) => new LeapSting(cID, owner, 7), Blowback.HasValidTargets),
            ((cID, owner) => new BruteUltimatum(cID, owner, 8), BruteUltimatum.HasValidTargets),
        ];

        public static AbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].constructor.Invoke(cID, owner);
        }
        public bool counterNegated { get; set; } = false;
        public bool IsCopy { get; set; } = false;

        public int TypeId { get; set; }
        public virtual AbilityKind Kind { get; } = AbilityKind.NormalAbility;

        public Game Game { get; set; }
        public Player Owner { get; set; }
        public int EffectId { get; set; }

        public AbilityCard? Fusion { get; set; }

        public int CardId { get; protected set; }

        public Bakugan User { get; set; }

        public virtual bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool BakuganIsValid(Bakugan user) =>
            IsActivateableByBakugan(user) && user.Owner == Owner && !user.UsedAbilityThisTurn;
        public virtual bool IsActivateableByBakugan(Bakugan user) =>
            throw new NotImplementedException();
        public virtual bool IsActivateableCounter() => IsActivateable();

        public static bool HasValidTargets(Bakugan user) => true;

        public virtual void Setup(bool asCounter)
        {
            Game.NewEvents[Owner.Id].Add(new JObject
            {
                { "Type", "StartSelection" },
                { "Count", 1 },
                { "Selections", new JArray {
                    new JObject {
                        { "SelectionType", "BF" },
                        { "Message", "INFO_ABILITYUSER" },
                        { "Ability", TypeId },
                        { "SelectionBakugans", new JArray(Game.BakuganIndex.Where(BakuganIsValid).Select(x =>
                            new JObject { { "Type", (int)x.Type },
                                { "Attribute", (int)x.Attribute },
                                { "Treatment", (int)x.Treatment },
                                { "Power", x.Power },
                                { "Owner", x.Owner.Id },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.AwaitingAnswers[Owner.Id] = Activate;
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public virtual void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPSIE!");
        }

        public virtual void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
        }

        public virtual void Dispose()
        {
            if (Owner.AbilityHand.Contains(this))
                Owner.AbilityHand.Remove(this);
            if (Game.ActiveZone.Contains(this))
                Game.ActiveZone.Remove(this);
            if (!IsCopy)
                Owner.AbilityGrave.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" }, { "Id", EffectId },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
            }
        }

        public virtual void Retract()
        {
            Game.ActiveZone.Remove(this);
            if (!IsCopy)
                Owner.AbilityHand.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" }, { "Id", EffectId },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
            }
        }

        public virtual void DoubleEffect() =>
            throw new NotImplementedException();

        public virtual void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
    }
}
