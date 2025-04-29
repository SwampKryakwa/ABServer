using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Abilities
{
    internal class VicariousVictim : AbilityCard
    {
        public VicariousVictim(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            TargetSelectors =
            [
                new BakuganSelector() { ClientType = "BG", ForPlayer = owner.Id, Message = "INFO_ABILITY_ADDTARGET", TargetValidator = x => x.Owner == Owner && x.InGrave()}
            ];
        }

        public override void TriggerEffect() => new IllusiveCurrentEffect(User, (TargetSelectors[0] as BakuganSelector).SelectedBakugan, TypeId, IsCopy).Activate();

        public override bool IsActivateableByBakugan(Bakugan user) => Game.CurrentWindow == ActivationWindow.Normal && user.OnField() && user.Type == BakuganType.Griffon;
    }

    internal class VicariousVictimEffect(Bakugan user, Bakugan selectedBakugan, int typeID, bool IsCopy)
    {
        public int TypeId { get; } = typeID;
        public Bakugan User = user;
        Bakugan selectedBakugan = selectedBakugan;
        Game game { get => User.Game; }

        public Player Owner { get; set; }
        bool IsCopy = IsCopy;

        public void Activate()
        {
            game.ThrowEvent(EventBuilder.ActivateAbilityEffect(TypeId, 0, User));

            if (User.Position is GateCard positionGate && selectedBakugan.Position is Player)
            {
                User.DestroyOnField(positionGate.EnterOrder);
                selectedBakugan.FromGrave(positionGate);
            }
        }
    }
}