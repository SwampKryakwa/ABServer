using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal enum ActiveType
    {
        Card,
        Effect
    }

    internal interface IActive
    {
        public int EffectId { get; set; }
        public int TypeId { get; }

        public ActiveType ActiveType { get; }

        public void Negate(bool asCounter = false);
    }

    internal class AbilityCard : IAbilityCard
    {
        public static (Func<int, Player, IAbilityCard> constructor, Func<Bakugan, bool> validTarget)[] AbilityCtrs =
        [
            //Set 1 Nova abilities
            ((cID, owner) => new FireWall(cID, owner, 0), FireWall.HasValidTargets),
            ((cID, owner) => new FireJudge(cID, owner, 1), FireJudge.HasValidTargets),
            ((cID, owner) => new FireTornado(cID, owner, 2), FireTornado.HasValidTargets),
            ((cID, owner) => new MoltenCore(cID, owner, 3), MoltenCore.HasValidTargets),

            //Set 1 Aqua abilities
            ((cID, owner) => new WaterRefrain(cID, owner, 4), WaterRefrain.HasValidTargets),
            ((cID, owner) => new TidalInsight(cID, owner, 5), TidalInsight.HasValidTargets),
            ((cID, owner) => new DiveMirage(cID, owner, 6), DiveMirage.HasValidTargets),
            ((cID, owner) => new LiquidForm(cID, owner, 7), LiquidForm.HasValidTargets),

            //Set 1 Darkon abilities
            ((cID, owner) => new GrandDown(cID, owner, 8), GrandDown.HasValidTargets),
            ((cID, owner) => new KillingCompanion(cID, owner, 9), KillingCompanion.HasValidTargets),
            ((cID, owner) => new OreganoMurder(cID, owner, 10), OreganoMurder.HasValidTargets),
            ((cID, owner) => new MergeShield(cID, owner, 11), MergeShield.HasValidTargets),

            //Set 1 Zephyros abilities
            ((cID, owner) => new AirBattle(cID, owner, 12), AirBattle.HasValidTargets),
            ((cID, owner) => new Blowback(cID, owner, 13), Blowback.HasValidTargets),
            ((cID, owner) => new JumpOver(cID, owner, 14), JumpOver.HasValidTargets),
            ((cID, owner) => new BlowAway(cID, owner, 15), BlowAway.HasValidTargets),

            //Set 1 Lumina abilities
            ((cID, owner) => new LuminaFreeze(cID, owner, 16), LuminaFreeze.HasValidTargets),
            ((cID, owner) => new LightningShield(cID, owner, 17), LightningShield.HasValidTargets),
            ((cID, owner) => new HolyLight(cID, owner, 18), HolyLight.HasValidTargets),
            ((cID, owner) => new ShadeAbility(cID, owner, 19), ShadeAbility.HasValidTargets),

            //Set 1 Subterra abilities
            ((cID, owner) => new Copycat(cID, owner, 20), Copycat.HasValidTargets),
            ((cID, owner) => new SpiritCanyon(cID, owner, 21), SpiritCanyon.HasValidTargets),
            ((cID, owner) => new DesertHole(cID, owner, 22), DesertHole.HasValidTargets),
            ((cID, owner) => new Plateau(cID, owner, 23), Plateau.HasValidTargets),

            //Set 1 Garrison abilities
            ((cID, owner) => new CoreForcement(cID, owner, 24), CoreForcement.HasValidTargets),
            ((cID, owner) => new LuminaAlliance(cID, owner, 25), LuminaAlliance.HasValidTargets),

            //Set 1 Griffon abilities
            ((cID, owner) => new WingBurst(cID, owner, 26), WingBurst.HasValidTargets),
            ((cID, owner) => new VicariousVictim(cID, owner, 27), VicariousVictim.HasValidTargets),
            ((cID, owner) => new DeafeningRoar(cID, owner, 28), DeafeningRoar.HasValidTargets),

            //Set 1 Mantis abilities
            ((cID, owner) => new Marionette(cID, owner, 29), Marionette.HasValidTargets),
            ((cID, owner) => new SlingBlazer(cID, owner, 30), SlingBlazer.HasValidTargets),
            ((cID, owner) => new TwinMachete(cID, owner, 31), TwinMachete.HasValidTargets),
            ((cID, owner) => new SliceCutter(cID, owner, 32), SliceCutter.HasValidTargets),

            //Set 1 Raptor abilities
            ((cID, owner) => new FrameFire(cID, owner, 33), FrameFire.HasValidTargets),
            ((cID, owner) => new BurstReturn(cID, owner, 34), BurstReturn.HasValidTargets),

            //Set 1 Saurus abilities
            ((cID, owner) => new SaurusGlow(cID, owner, 35), SaurusGlow.HasValidTargets),
            ((cID, owner) => new SaurusRage(cID, owner, 36), SaurusRage.HasValidTargets),

            //Set 1 Centipede abilities
            ((cID, owner) => new Attractor(cID, owner, 37), Attractor.HasValidTargets),
            ((cID, owner) => new DraggedIntoDarkness(cID, owner, 38), DraggedIntoDarkness.HasValidTargets),
            
            //Set 1 Serpent abilities
            ((cID, owner) => new SerpentSqueeze(cID, owner, 39), SerpentSqueeze.HasValidTargets),
            ((cID, owner) => new CinderCoil(cID, owner, 40), CinderCoil.HasValidTargets),
            ((cID, owner) => new BindingWhirlwind(cID, owner, 41), BindingWhirlwind.HasValidTargets),

            //Set 1 Fairy abilities
            ((cID, owner) => new ScarletTwister(cID, owner, 42), ScarletTwister.HasValidTargets),
            ((cID, owner) => new DarkMirage(cID, owner, 42), DarkMirage.HasValidTargets),
            ((cID, owner) => new PowderVeil(cID, owner, 44), PowderVeil.HasValidTargets),

            //Set 1 Elephant abilities
            ((cID, owner) => new NoseSlap(cID, owner, 45), NoseSlap.HasValidTargets)
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].constructor.Invoke(cID, owner);
        }
        public bool counterNegated { get; set; } = false;
        public bool IsCopy { get; set; } = false;

        public int TypeId { get; set; }

        public Game Game { get; set; }
        public Player Owner { get; set; }
        public ActiveType ActiveType { get; } = ActiveType.Card;
        public int EffectId { get; set; }

        public IAbilityCard FusedTo { get; set; }
        public IAbilityCard Fusion { get; set; }

        public int CardId { get; protected set; }

        public Bakugan User { get; set; }

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPSIE!");
        }

        public new void DoNotAffect(Bakugan bakugan)
        {
            if (User == bakugan)
                User = Bakugan.GetDummy();
        }

        public void Dispose()
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

        public void Retract()
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

        public void DoubleEffect() =>
            throw new NotImplementedException();

        public static bool HasValidTargets(Bakugan user) =>
            true;

        public void Negate(bool asCounter)
        {
            if (asCounter)
                counterNegated = true;
        }
#pragma warning restore CS8618
    }

    interface IAbilityCard : IActive
    {
        public Game Game { get; protected set; }
        public int CardId { get; }
        public Player Owner { get; protected set; }

        protected bool counterNegated { get; set; }
        public bool IsCopy { get; set; }

        public bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool IsActivateableFusion(Bakugan user) =>
            throw new NotImplementedException();
        public bool IsActivateableCounter() => IsActivateable();

        public bool BakuganIsValid(Bakugan user) =>
            IsActivateableFusion(user) && user.Owner == Owner && !user.UsedAbilityThisTurn;

        public Bakugan User { get; protected set; }

        public void Setup(bool asCounter)
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

            Game.awaitingAnswers[Owner.Id] = Activate;
        }

        public IAbilityCard FusedTo { get; set; }
        public IAbilityCard Fusion { get; set; }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;
            FusedTo = parentCard;

            if (parentCard != null) parentCard.Fusion = this;

            Game.CheckChain(Owner, this, user);
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public void DoubleEffect();

        public void Resolve();

        public void DoNotAffect(Bakugan bakugan);

        public static bool HasValidTargets(Bakugan user) =>
            true;
    }
}
