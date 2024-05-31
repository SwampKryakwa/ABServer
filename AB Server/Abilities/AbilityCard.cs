namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {

        static Func<int, Player, IAbilityCard>[] AbilityCtrs =
        [
            (x, y) => new FireJudge(x, y),
            (x, y) => new FireTornado(x, y),
            (x, y) => new Backfire(x, y),
            (x, y) => new RapidFire(x, y),
            (x, y) => new RapidLight(x, y),
            (x, y) => new ClayArmor(x, y),
            (x, y) => new MagmaSurface(x, y),
            (x, y) => new DesertVortex(x, y),
            (x, y) => new SpiritCanyon(x, y),
            (x, y) => new LightHelix(x, y),
            (x, y) => new LightningTornado(x, y),
            (x, y) => new HaosFreeze(x, y),
            (x, y) => new ShiningBrilliance(x, y),
            (x, y) => new ColourfulDeath(x, y),
            (x, y) => new CyclingMadness(x, y),
            (x, y) => new ChainsDes(x, y),
            (x, y) => new JudgementNight(x, y),
            (x, y) => new Uptake(x, y),
            (x, y) => new TornadoWall(x, y),
            (x, y) => new BlindJudge(x, y),
            (x, y) => new Tsunami(x, y)
        ];

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }

        private protected Game Game;
        public Player Owner { get; protected set; }

        public int CardId { get; protected set; }

        public Bakugan User { get; protected set; }

        public int TypeId =>
            throw new NotImplementedException();

        protected bool BakuganIsValid(Bakugan user) =>
            IsActivateableFusion(user) && user.Owner == Owner && !user.UsedAbilityThisTurn;

        public bool IsActivateable() =>
            Game.BakuganIndex.Any(BakuganIsValid);
        public bool IsActivateableFusion(Bakugan user) =>
            throw new NotImplementedException();
        public bool IsActivateableCounter() => IsActivateable();

        protected void Dispose()
        {
            Owner.AbilityHand.Remove(this);
            Owner.AbilityGrave.Add(this);
        }

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int TypeId { get; }

        public int CardId { get; }

        public void Setup(bool asCounter) =>
            throw new NotImplementedException();

        public void SetupFusion(IAbilityCard parentCard) =>
            throw new NotImplementedException();

        public Bakugan User { get; }

        public void Resolve() =>
            throw new NotImplementedException();

        //public void SetupFusion(bool asCounter) => Setup();

        public bool IsActivateable();
        public bool IsActivateableFusion(Bakugan user);
        public bool IsActivateableCounter();
    }

    interface INegatable
    {
        public int TypeId { get; }
        public Player Owner { get; }
        public void Negate();
    }
}
