using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace AB_Server.Abilities
{
    internal class DiveMirage : AbilityCard
    {
        public DiveMirage(int cID, Player owner, int typeId) : base(cID, owner, typeId)
        {
            CondTargetSelectors =
            [
                new GateSelector() { ClientType = "GF", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_DESTINATIONTARGET", TargetValidator = x => x == Game.GateSetList.Last() }
            ];
        }

        public override void TriggerEffect()
        {
            GenericEffects.MoveBakuganEffect(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, new JObject() { ["MoveEffect"] = "Submerge" });
            if (Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Aqua)))
                new DiveMirageMarker(User, (CondTargetSelectors[0] as GateSelector)!.SelectedGate, Owner, Game, TypeId, Kind, IsCopy).Activate();
        }

        public override bool IsActivateableByBakugan(Bakugan user) =>
            Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Aqua) && user.OnField();
    }

    internal class DiveMirageMarker(Bakugan user, GateCard target, Player owner, Game game, int typeId, CardKind kind, bool isCopy) : IActive
    {
        public int EffectId { get; set; } = game.NextEffectId++;

        public int TypeId { get; } = typeId;

        public CardKind Kind { get; } = kind;

        Bakugan IActive.User { get; set; } = user;
        Player IActive.Owner { get; set; } = owner;

        public void Activate()
        {
            game.ActiveZone.Add(this);

            game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));
            target.OpenBlocking.Add(this);

            game.TurnEnd += StopEffect;
        }

        public void Negate(bool asCounter = false)
        {
            StopEffect();
        }

        void StopEffect()
        {
            game.ActiveZone.Remove(this);

            game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));
            target.OpenBlocking.Remove(this);

            game.TurnEnd -= StopEffect;
        }
    }
}
