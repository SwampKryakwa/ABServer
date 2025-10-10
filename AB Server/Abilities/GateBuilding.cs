using AB_Server.Gates;
using AB_Server.Gates.SpecialGates;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace AB_Server.Abilities;

internal class GateBuilding : AbilityCard
{
    public GateBuilding(int cID, Player owner, int typeId) : base(cID, owner, typeId)
    {
        ResTargetSelectors =
        [
            new YesNoSelector { Message = "INFO_WANTSETGATE", ForPlayer = (p) => p == Owner, Condition = () => Owner.GateHand.Count != 0 },
            new GateSelector { ClientType = "GH", ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_GATETARGET", TargetValidator = g => Owner.GateHand.Contains(g) && !pickedGates.Contains(g), Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes },
            new GateSlotSelector { ForPlayer = (p) => p == Owner, Message = "INFO_ABILITY_TARGET", Condition = () => (ResTargetSelectors[0] as YesNoSelector)!.IsYes }
        ];
    }

    List<GateCard> pickedGates = [];
    List<(int X, int Y)> pickedPositions = [];

    public override void Resolve()
    {
        Game.OnAnswer[Owner.Id] = () =>
        {
            int attribute = (int)Game.PlayerAnswers[Owner.Id]!["array"][0]["attribute"];
            int newCid = Game.GateIndex.Count;
            GateCard newCard = attribute switch
            {
                0 => new GateOfNova120(newCid, Owner),
                1 => new GateOfAqua120(newCid, Owner),
                2 => new GateOfDarkon120(newCid, Owner),
                3 => new GateOfZephyros120(newCid, Owner),
                4 => new GateOfLumina120(newCid, Owner),
                5 => new GateOfSubterra120(newCid, Owner),
                _ => throw new NotImplementedException(),
            };
            Game.GateIndex.Add(newCard);
            Owner.GateHand.Add(newCard);
            Game.ThrowEvent(new JObject
            {
                ["Type"] = "GateAddedToHand",
                ["Owner"] = newCard.Owner.Id,
                ["Kind"] = (int)newCard.Kind,
                ["CardType"] = newCard.TypeId,
                ["CID"] = newCard.CardId
            });
            base.Resolve();
        };
        Game.ThrowEvent(Owner.Id, EventBuilder.SelectionBundler(false && Game.CurrentWindow == ActivationWindow.Normal, EventBuilder.AttributeSelectionEvent("INFO_PICKATTRIBUTE", Enum.GetValues<Attribute>())));
    }

    public override void TriggerEffect()
    {
        if (!(ResTargetSelectors[0] as YesNoSelector)!.IsYes)
        {
            for (int i = 0; i < pickedGates.Count; i++)
            {
                pickedGates[i].Set((byte)pickedPositions[i].X, (byte)pickedPositions[i].Y);
            }
        }
        else
        {
            pickedGates.Add((ResTargetSelectors[1] as GateSelector)!.SelectedGate);
            pickedPositions.Add((ResTargetSelectors[2] as GateSlotSelector)!.SelectedSlot);
            (ResTargetSelectors[1] as GateSelector)!.SelectedGate.RemoveFromHand();
            (ResTargetSelectors[0] as YesNoSelector)!.IsYes = false;
            base.Resolve();
        }
    }

    public override bool IsActivateableByBakugan(Bakugan user) =>
        Game.CurrentWindow == ActivationWindow.Normal && user.IsAttribute(Attribute.Subterra) && user.OnField();

    [ModuleInitializer]
    internal static void Init() => Register(25, CardKind.NormalAbility, (cID, owner) => new GateBuilding(cID, owner, 25));
}
