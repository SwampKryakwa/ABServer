using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    public enum AbilityKind : byte
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

            //Set 2 attribute abilities
            ((cID, owner) => new FireWall(cID, owner, 9), FireWall.HasValidTargets),
            ((cID, owner) => new Tunneling(cID, owner, 10), Tunneling.HasValidTargets),
            ((cID, owner) => new LightningTornado(cID, owner, 11), LightningTornado.HasValidTargets),
            ((cID, owner) => new BloomOfAgony(cID, owner, 12), BloomOfAgony.HasValidTargets),
            ((cID, owner) => new IllusiveCurrent(cID, owner, 13), IllusiveCurrent.HasValidTargets),
            ((cID, owner) => new AirBattle(cID, owner, 14), AirBattle.HasValidTargets),

            //Set 2 bakugan abilities
            ((cID, owner) => new DefiantCounterattack(cID, owner, 15), DefiantCounterattack.HasValidTargets),
            ((cID, owner) => new CutInSlayer(cID, owner, 16), CutInSlayer.HasValidTargets),
            ((cID, owner) => new NoseSlap(cID, owner, 17), NoseSlap.HasValidTargets),
            ((cID, owner) => new SaurusGlow(cID, owner, 18), SaurusGlow.HasValidTargets),
            ((cID, owner) => new Dimension4(cID, owner, 19), Dimension4.HasValidTargets),
        ];

        public static AbilityCard CreateCard(Player owner, int cID, int type) =>
            AbilityCtrs[type].constructor.Invoke(cID, owner);
        public bool counterNegated { get; set; } = false;
        public bool IsCopy { get; set; } = false;

        public int TypeId { get; set; }
        public virtual AbilityKind Kind { get; } = AbilityKind.NormalAbility;

        public Game Game { get; set; }
        public Player Owner { get; set; }
        public int EffectId { get; set; }

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
            Game.NewEvents[Owner.Id].Add(EventBuilder.SelectionBundler(
                EventBuilder.FieldBakuganSelection("INFO_ABILITYUSER", TypeId, (int)Kind, Owner.BakuganOwned.Where(BakuganIsValid))
                ));

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
