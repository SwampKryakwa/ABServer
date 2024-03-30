namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {
        private protected Func<Bakugan, bool> BakuganIsValid;

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
        public Player Owner;

        public int CID { get; protected set; }

        public int GetTypeID() =>
            throw new NotImplementedException();

        public void Activate() =>
            throw new NotImplementedException();

        public void ActivateCounter() =>
            throw new NotImplementedException();

        public void ActivateFusion(IAbilityCard fusedWith, Bakugan user) =>
            throw new NotImplementedException();

        public new void Resolve() =>
            throw new NotImplementedException();

        protected void Dispose()
        {
            Owner.AbilityHand.Remove(this);
            Owner.AbilityGrave.Add(this);
        }

        public bool IsActivateable() => Game.BakuganIndex.Any(BakuganIsValid);

        public bool IsActivateable(bool asFusion) => IsActivateable();

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int CID { get; }
        public int GetTypeID();
        public void Activate();
        public void ActivateCounter();
        public void ActivateFusion(IAbilityCard fusedWith, Bakugan user);
        public new void Resolve();

        public bool IsActivateable();
        public bool IsActivateable(bool asFusion);
    }

    interface INegatable
    {
        public int TypeID { get; }
        public Player GetOwner();
        public void Negate(bool asCounter);
    }
}
