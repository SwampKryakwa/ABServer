using Newtonsoft.Json.Linq;

namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {
        static Func<int, Player, IAbilityCard>[] AbilityCtrs =
        [
            (cID, owner) => new FireJudge(cID, owner),
            (cID, owner) => new FireTornado(cID, owner),
            (cID, owner) => new Backfire(cID, owner),
            (cID, owner) => new RapidFire(cID, owner),
            (cID, owner) => new RapidLight(cID, owner),
            (cID, owner) => new ClayArmor(cID, owner),
            (cID, owner) => new MagmaProminence(cID, owner),
            (cID, owner) => new DesertVortex(cID, owner),
            (cID, owner) => new SpiritCanyon(cID, owner),
            (cID, owner) => new LightSpiral(cID, owner),
            (cID, owner) => new LightningTornado(cID, owner),
            (cID, owner) => new HaosFreeze(cID, owner),
            (cID, owner) => new ShiningBrilliance(cID, owner),
            (cID, owner) => new OreganoMurder(cID, owner),
            (cID, owner) => new CyclingMadness(cID, owner),
            (cID, owner) => new ChainsDes(cID, owner),
            (cID, owner) => new JudgementNight(cID, owner),
            (cID, owner) => new Absorb(cID, owner),
            (cID, owner) => new TornadoWall(cID, owner),
            (cID, owner) => new BlindJudge(cID, owner),
            (cID, owner) => new TsunamiWave(cID, owner),
            (cID, owner) => new SerpentSqueeze(cID, owner),
            (cID, owner) => new CinderCoil(cID, owner)
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }

        public int TypeId =>
            throw new NotImplementedException();

        public Game Game { get; set; }
        public Player Owner { get; set; }

        public int CardId { get; protected set; }

        protected bool counterNegated = false;

        public Bakugan User { get; set; }

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPSIE!");
        }

        public void Negate()
        {
            counterNegated = true;
        }

        protected void Dispose()
        {
            Owner.AbilityHand.Remove(this);
            Owner.AbilityGrave.Add(this);
        }

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int TypeId =>
            throw new NotImplementedException();

        public Game Game { get; protected set; }
        public int CardId { get; }
        public Player Owner { get; protected set; }

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
            Game.AbilityChain.Add(this);
            Game.NewEvents[Owner.ID].Add(new JObject
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
                                { "Owner", x.Owner.ID },
                                { "BID", x.BID } })) }
                    }
                } }
            });

            Game.awaitingAnswers[Owner.ID] = Activate;
        }

        public void SetupFusion(IAbilityCard parentCard, Bakugan user)
        {
            User = user;

            Game.AbilityChain.Add(this);
            Game.CheckChain(Owner, this, user);
        }

        public void Activate()
        {
            User = Game.BakuganIndex[(int)Game.IncomingSelection[Owner.ID]["array"][0]["bakugan"]];

            Game.CheckChain(Owner, this, User);
        }

        public void Resolve()
        {
            Console.WriteLine("OOOOOOOOOOOOOOOOOPS!");
        }
    }

    interface INegatable
    {
        public int TypeId { get; }
        public Player Owner { get; }
        public void Negate();
    }
}
