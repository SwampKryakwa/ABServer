using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class Enforcement(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect() =>
            new EnforcementEffect(User, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.Owner.BakuganOwned.Any(x => x.Type == BakuganType.Garrison) && user.OnField();
    }

    internal class EnforcementEffect(Bakugan user, int typeID, bool IsCopy) : IActive
    {
        public int TypeId { get; } = typeID;
        public int EffectId { get; set; } = user.Game.NextEffectId++;
        public CardKind Kind { get; } = CardKind.NormalAbility;
        public Bakugan User { get; set; } = user;
        Game game { get => User.Game; }
        Boost currentBoost;

        public Player Owner { get; set; } = user.Owner;
        bool IsCopy = IsCopy;

        public void Activate()
        {
            int team = User.Owner.SideID;
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));
            game.ThrowEvent(EventBuilder.AddEffectToActiveZone(this, IsCopy));

            currentBoost = new Boost(50);
            User.Boost(currentBoost, this);

            game.BakuganDestroyed += OnBakuganLeaveField;
            game.BakuganReturned += OnBakuganLeaveField;
        }

        private void OnBakuganLeaveField(Bakugan target, byte owner)
        {
            if (target == User)
            {
                currentBoost = new Boost(50);
                User.Boost(currentBoost, this);
            }
        }

        public void Negate(bool asCounter)
        {
            game.ActiveZone.Remove(this);

            game.BakuganDestroyed -= OnBakuganLeaveField;
            game.BakuganReturned -= OnBakuganLeaveField;

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
