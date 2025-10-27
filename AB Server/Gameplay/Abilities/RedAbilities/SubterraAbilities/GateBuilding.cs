using AB_Server.Gates;
using AB_Server.Gates.SpecialGates;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;


/*
 * Gate Building
 * REQUIREMENT: Choose your SUBTERRA bakugan on the field to use. 
 * EFFECT: Name any gate card. Create a copy of that card into your hand. Then you can place 1 gate card on the field. 
 */
internal class GateBuilding : AbilityCard
{
    public GateBuilding(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new TypeSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", SelectableKinds = [CardKind.CommandGate, CardKind.SpecialGate] },
        ];
    }

    public override void TriggerEffect()
    {
        if (ResTargetSelectors[0] is TypeSelector typeSelector)
        {
            GateCard newCard = (CardKind)typeSelector.SelectedKind switch
            {
                CardKind.CommandGate => GateCard.CreateCard(Owner, Game.GateIndex.Count, typeSelector.SelectedType),
                CardKind.SpecialGate => GateCard.CreateSpecialCard(Owner, Game.GateIndex.Count, typeSelector.SelectedType),
                _ => throw new InvalidOperationException("Invalid card kind selected.")
            };
            Game.GateIndex.Add(newCard);
            Owner.GateHand.Add(newCard);
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "GateAddedToHand",
                ["Owner"] = newCard.Owner.PlayerId,
                ["Kind"] = (int)newCard.Kind,
                ["CardType"] = newCard.TypeId,
                ["CID"] = newCard.CardId
            });
            ResTargetSelectors =
            [
                new YesNoSelector { Message = "INFO_WANTSETGATE", ForPlayer = (p) => p == Owner, Condition = () => Owner.GateHand.Count != 0 },
                new GateSelector { ClientType = "GH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = g => Owner.GateHand.Contains(g) && !pickedGates.Contains(g), Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes },
                new GateSlotSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
            ];
            Resolve();
        }
        else if (ResTargetSelectors[0] is YesNoSelector yesNoSelector && yesNoSelector.IsYes)
        {
            GateCard selectedGate = (ResTargetSelectors[1] as GateSelector)!.SelectedGate;
            (int posX, int posY) = (ResTargetSelectors[2] as GateSlotSelector)!.SelectedSlot;
            selectedGate.Set((byte)posX, (byte)posY);
        }
    }

    public override bool UserValidator(Bakugan user) =>
        user.IsAttribute(Attribute.Subterra) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(25, CardKind.NormalAbility, (cID, owner) => new GateBuilding(cID, owner, 25));
}
