using AB_Server.Gates;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class PowerCharge(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
    {
        public override void TriggerEffect()
        {
            new PowerChargeMarker(User, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Nova) && user.OnField() && (!user.InBattle);

        [ModuleInitializer]
        internal static void Init() => Register(39, CardKind.NormalAbility, (cID, owner) => new PowerCharge(cID, owner, 39));
    }

    internal class PowerChargeMarker(Bakugan user, bool isCopy) : IActive
    {
        public int EffectId { get; set; }

        public int TypeId { get; } = 39;

        public CardKind Kind { get; } = CardKind.NormalAbility;

        public Bakugan User { get; set; } = user;
        public Player Owner { get; set; }

        public void Activate()
        {
            User.Game.ActiveZone.Add(this);

            User.Game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

            User.Game.BattleAboutToStart += OnBattleStart;
        }

        public void OnBattleStart(GateCard position)
        {
            if (User.Position == position)
            {
                User.Boost(200, this);
                CeaseMarker();
            }
        }

        public void Negate(bool asCounter = false)
        {
            CeaseMarker();
        }

        public void CeaseMarker()
        {
            User.Game.ActiveZone.Remove(this);

            User.Game.BattleAboutToStart -= OnBattleStart;

            User.Game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
        }
    }
}
