using AB_Server.Gates;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace AB_Server.Abilities
{
    internal class DiveMirage(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
    {
        public override void TriggerEffect()
        {
            new MoveBakuganEffect(User, User, Game.GateSetList.Last(), TypeId, (int)Kind, new JObject() { ["MoveEffect"] = "Submerge" }).Activate();    
            if (Owner.BakuganOwned.All(x => x.IsAttribute(Attribute.Aqua)))
                new DiveMirageMarker(User, Game.GateSetList.Last(), Owner, Game, TypeId, Kind, IsCopy).Activate();
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
