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
        static Func<int, Player, IAbilityCard>[] AbilityCtrs =
        [
            //Set 1 Nova abilities
            (cID, owner) => new FireWall(cID, owner, 0),
            (cID, owner) => new FireJudge(cID, owner, 1),
            (cID, owner) => new FireTornado(cID, owner, 2),
            (cID, owner) => new MoltenCore(cID, owner, 3),

            //Set 1 Aqua abilities
            (cID, owner) => new WaterRefrain(cID, owner, 4),
            (cID, owner) => throw new NotImplementedException(), //5
            (cID, owner) => new DiveMirage(cID, owner, 6),
            (cID, owner) => new LiquidForm(cID, owner, 7),

            //Set 1 Darkon abilities
            (cID, owner) => new GrandDown(cID, owner, 8),
            (cID, owner) => new KillingCompanion(cID, owner, 9),
            (cID, owner) => new OreganoMurder(cID, owner, 10),
            (cID, owner) => new MergeShield(cID, owner, 11),

            //Set 1 Zephyros abilities
            (cID, owner) => new AirBattle(cID, owner, 12),
            (cID, owner) => new Blowback(cID, owner, 13),
            (cID, owner) => new JumpOver(cID, owner, 14),
            (cID, owner) => new BlowAway(cID, owner, 15),

            //Set 1 Lumina abilities
            (cID, owner) => new LuminaFreeze(cID, owner, 16),
            (cID, owner) => new LightningShield(cID, owner, 17),
            (cID, owner) => new HolyLight(cID, owner, 18),
            (cID, owner) => new ShadeAbility(cID, owner, 19),

            //Set 1 Subterra abilities
            (cID, owner) => throw new NotImplementedException(), //20
            (cID, owner) => new SpiritCanyon(cID, owner, 21),
            (cID, owner) => new DesertHole(cID, owner, 22),
            (cID, owner) => throw new NotImplementedException(), //23

            //Set 1 Garrison abilities
            (cID, owner) => new CoreForcement(cID, owner, 24),
            (cID, owner) => throw new NotImplementedException(),//25

            //Set 1 Griffon abilities
            (cID, owner) => new WingBurst(cID, owner, 26),
            (cID, owner) => new VicariousVictim(cID, owner, 27),
            (cID, owner) => new DeafeningRoar(cID, owner, 28),

            //Set 1 Mantis abilities
            (cID, owner) => throw new NotImplementedException(), //29
            (cID, owner) => throw new NotImplementedException(), //30
            (cID, owner) => new TwinMachete(cID, owner, 31),
            (cID, owner) => new SliceCutter(cID, owner, 32),

            //Set 1 Raptor abilities
            (cID, owner) => throw new NotImplementedException(), //33
            (cID, owner) => new BurstReturn(cID, owner, 34),

            //Set 1 Saurus abilities
            (cID, owner) => new SaurusGlow(cID, owner, 35),
            (cID, owner) => new SaurusRage(cID, owner, 36),

            //Set 1 Centipede abilities
            (cID, owner) => throw new NotImplementedException(), //37
            (cID, owner) => throw new NotImplementedException(), //38
            
            //Set 1 Serpent abilities
            (cID, owner) => new SerpentSqueeze(cID, owner, 39),
            (cID, owner) => new CinderCoil(cID, owner, 40),
            (cID, owner) => new BindingWhirlwind(cID, owner, 41),

            //Set 1 Fairy abilities
            (cID, owner) => new ScarletTwister(cID, owner, 42),
            (cID, owner) => new DarkMirage(cID, owner, 42),
            (cID, owner) => new PowderVeil(cID, owner, 44),

            //Set 1 Elephant abilities
            (cID, owner) => new NoseSlap(cID, owner, 45),
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }
        public bool counterNegated { get; set; } = false;

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

        public void Dispose()
        {
            if (Owner.AbilityHand.Contains(this))
                Owner.AbilityHand.Remove(this);
            if (Game.ActiveZone.Contains(this))
                Game.ActiveZone.Remove(this);
            Owner.AbilityGrave.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
            }
        }

        public void Retract()
        {
            Game.ActiveZone.Remove(this);
            Owner.AbilityHand.Add(this);

            for (int i = 0; i < Game.NewEvents.Length; i++)
            {
                Game.NewEvents[i].Add(new()
                {
                    { "Type", "AbilityRemovedActiveZone" },
                    { "Card", TypeId },
                    { "Owner", Owner.Id }
                });
            }
        }

        public void DoubleEffect() =>
            throw new NotImplementedException();

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
            parentCard.Fusion = this;

            Game.CheckChain(Owner, this, user);
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.Id]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public void DoubleEffect();

        public void Resolve();

        public void Negate(bool asCounter);
    }
}
