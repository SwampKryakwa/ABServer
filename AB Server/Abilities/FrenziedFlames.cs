using System.Runtime.CompilerServices;

namespace AB_Server.Abilities
{
    internal class FrenziedFlames(int cId, Player owner, int typeId) : AbilityCard(cId, owner, typeId)
    {
        public override void TriggerEffect()
        {
            foreach (var bak in Owner.BakuganOwned.Where(x => x.OnField()))
                bak.Boost(300, this);
        }
        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Intermediate && user.JustEndedBattle && !user.BattleEndedInDraw && user.OnField() && user.IsAttribute(Attribute.Nova);

        [ModuleInitializer]
        internal static void Init() => Register(38, CardKind.NormalAbility, (cID, owner) => new ScarletWaltz(cID, owner, 38));
    }
}
