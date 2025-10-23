using AB_Server.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server;

internal partial class Bakugan
{
    //Power state
    public List<Boost> Boosts = [];
    public List<Boost> ContinuousBoosts = [];

    //Attribute state
    public List<AttributeState> attributeChanges = [];

    //Position state
    public IBakuganContainer Position = owner;

    //Battle state
    public bool Defeated = false;
    public byte DestructionTurn = 0;
    public GateCard DestroyedOn;
    public bool JustEndedBattle = false;
    public bool BattleEndedInDraw = false;

    //Other state
    public bool Frenzied = false;

    //Blockers
    public List<object> AbilityBlockers = [];

    //Shorthands
    public int Power
    {
        get => BasePower + Boosts.Sum(b => b.Value) + ContinuousBoosts.Sum(b => b.Value);
    }
    public int AdditionalPower
    {
        get => Boosts.Sum(b => b.Value) + ContinuousBoosts.Sum(b => b.Value);
    }
    public IEnumerable<Attribute> CurrentAttributes
    {
        get => attributeChanges.Count == 0 ? [BaseAttribute] : attributeChanges[^1].Attributes;
    }


    public bool IsAttribute(Attribute attr) =>
        attributeChanges.Count == 0 ? BaseAttribute == attr : attributeChanges[^1].IsAttribute(attr);

    public bool InBattle
    {
        get => Position is GateCard gatePosition && gatePosition.IsBattleGoing;
    }

    //Position checks
    public bool OnField() =>
        Position is GateCard;

    public bool InHand() =>
        Position is Player;

    public bool InDrop() =>
        Position is BakuganDrop;
}
