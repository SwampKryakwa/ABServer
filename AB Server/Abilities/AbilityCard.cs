namespace AB_Server.Abilities
{
    internal class AbilityCard : IAbilityCard
    {
        static Func<int, Player, IAbilityCard>[] AbilityCtrs = new Func<int, Player, IAbilityCard>[]
        {
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
            (x, y) => new BlindJudge(x, y)
        };

        public static IAbilityCard CreateCard(Player owner, int cID, int type)
        {
            return AbilityCtrs[type].Invoke(cID, owner);
        }
        private protected Game game;
        private protected Player owner;

        public int CID { get; protected set; }
        public int GetTypeID()
        {
            throw new NotImplementedException();
        }
        public void Activate()
        {
            throw new NotImplementedException();
        }
        public void ActivateCounter()
        {
            throw new NotImplementedException();
        }
        public void ActivateFusion()
        {
            throw new NotImplementedException();
        }
        protected void Dispose()
        {
            owner.AbilityHand.Remove(this);
            owner.AbilityGrave.Add(this);
        }

        public void Resolve()
        {
            throw new NotImplementedException();
        }

        private protected int iter = 0;

        public bool Iter()
        {
            iter++;
            if (iter == game.PlayerCount)
            {
                iter = 0;
                return true;
            }
            return false;
        }

        public bool IsActivateable()
        {
            throw new NotImplementedException();
        }
        public bool IsActivateable(bool asFusion)
        {
            throw new NotImplementedException();
        }

#pragma warning restore CS8618
    }

    interface IAbilityCard
    {
        public int CID { get; }
        public int GetTypeID();
        public void Activate();
        public void ActivateCounter();
        public void ActivateFusion();

        public bool Iter();
        public void Resolve();

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
