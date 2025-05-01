namespace AB_Server.Abilities.Fusions
{
    internal class Alliance(int cID, Player owner) : FusionAbility(cID, owner, 9, typeof(Enforcement))
    {
        public override void TriggerEffect() =>
            new AllianceEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && user.Type == BakuganType.Garrison && Game.CurrentWindow == ActivationWindow.Normal;
    }

    internal class AllianceEffect(Bakugan user, int typeID, bool IsCopy) : IActive
    {
        public int TypeId { get; } = typeID;
        public int EffectId { get; set; } = user.Game.NextEffectId++;
        public CardKind Kind { get; } = CardKind.FusionAbility;
        public Bakugan User { get; set; } = user;
        Game game { get => User.Game; }
        Dictionary<int, Boost> currentBoosts = [];

        public Player Owner { get; set; } = user.Owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            int team = User.Owner.SideID;
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 1, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            foreach (var bakugan in Owner.BakuganOwned)
            {
                if (bakugan == User) continue;
                var currentBoost = new Boost(80);
                currentBoosts.Add(bakugan.BID, currentBoost);
                bakugan.ContinuousBoost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            foreach (var currentBoost in currentBoosts.Keys)
                if (currentBoosts[currentBoost].Active)
                {
                    currentBoosts[currentBoost].Active = false;
                    game.BakuganIndex[currentBoost].RemoveBoost(currentBoosts[currentBoost], this);
                }

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
        }
    }
}
