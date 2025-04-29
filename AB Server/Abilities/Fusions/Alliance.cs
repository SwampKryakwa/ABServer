using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities.Fusions
{
    internal class Alliance(int cID, Player owner) : FusionAbility(cID, owner, 9, typeof(Enforcement))
    {
        public override void TriggerEffect() =>
            new AllianceEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            user.OnField() && user.IsPartner && user.Type == BakuganType.Garrison && Game.CurrentWindow == ActivationWindow.Normal && Owner.BakuganOwned.Select(x => x.BaseAttribute).Distinct().Count() == Owner.BakuganOwned.Count && Owner.BakuganOwned.Select(x => x.Type).Distinct().Count() == Owner.BakuganOwned.Count && Owner.BakuganOwned.Select(x => x.BasePower).Distinct().Count() == Owner.BakuganOwned.Count;
    }

    internal class AllianceEffect(Bakugan user, int typeID, bool IsCopy) : IActive
    {
        public int TypeId { get; } = typeID;
        public int EffectId { get; set; } = user.Game.NextEffectId++;
        public CardKind Kind { get; } = CardKind.NormalAbility;
        public Bakugan User { get; set; } = user;
        Game game { get => User.Game; }
        Dictionary<int, Boost> currentBoosts;

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
                var currentBoost = new Boost(50);
                currentBoosts.Add(bakugan.BID, currentBoost);
                bakugan.Boost(currentBoost, this);
            }

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;
        }

        private void OnBakuganLeaveField(Bakugan target, byte owner)
        {
            if (target == User)
            {
                currentBoosts[target.BID] = new Boost(50);
                User.Boost(currentBoosts[target.BID], this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

            foreach (var currentBoost in currentBoosts.Values)
                if (currentBoost.Active)
                {
                    currentBoost.Active = false;
                    User.RemoveBoost(currentBoost, this);
                }

            game.ThrowEvent(new()
            {
                ["Type"] = "EffectRemovedActiveZone",
                ["Id"] = EffectId
            });
        }
    }
}
