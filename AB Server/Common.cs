using AB_Server.Abilities;
using AB_Server.Gates;
using System.Linq;

namespace AB_Server
{
    internal interface IActive
    {
        public int EffectId { get; set; }
        public int TypeId { get; }
        public CardKind Kind { get; }
        public Bakugan User { get; set; }

        public Player Owner { get; set; }

        public void Negate(bool asCounter = false);
    }

    internal interface IChainable
    {
        public void Resolve();
    }

    interface IBakuganContainer
    {
        List<Bakugan> Bakugans { get; }

        public void Remove(Bakugan bakugan)
        {
            Bakugans.Remove(bakugan);
        }

        public void Add(Bakugan bakugan)
        {
            Bakugans.Add(bakugan);
        }
    }

    abstract class Selector
    {
        public string ClientType;
        public string Message;
        public Func<Player, bool> ForPlayer;
        public Func<bool> Condition = () => true;
        public abstract bool HasValidTargets(Game game);
    }

    class YesNoSelector : Selector
    {
        public bool IsYes;
        public override bool HasValidTargets(Game game) { return true; }
    }

    class OptionSelector : Selector
    {
        public int OptionCount;
        public int SelectedOption;

        public override bool HasValidTargets(Game game) { return true; }
    }

    class GateSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator = (x) => true;
        public GateCard SelectedGate;

        public override bool HasValidTargets(Game game) { return game.GateIndex.Any(TargetValidator); }
    }

    class BakuganSelector : Selector
    {
        public Func<Bakugan, bool> TargetValidator = (x) => true;
        public Bakugan SelectedBakugan;

        public override bool HasValidTargets(Game game) { return game.BakuganIndex.Any(TargetValidator); }
    }

    class AbilitySelector : Selector
    {
        public Func<AbilityCard, bool> TargetValidator = (x) => true;
        public AbilityCard SelectedAbility;

        public override bool HasValidTargets(Game game) { return game.AbilityIndex.Any(TargetValidator); }
    }

    class ActiveSelector : Selector
    {
        public Func<IActive, bool> TargetValidator = (x) => true;
        public IActive SelectedActive;

        public override bool HasValidTargets(Game game) { return game.ActiveZone.Any(TargetValidator); }
    }

    class GateSlotSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator = (x) => true;
        public (int X, int Y) SelectedSlot;

        public override bool HasValidTargets(Game game) { return game.Field.Cast<GateCard?>().Count(x => x is null) > 4; }
    }

    class MultiGateSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator = (x) => true;
        public GateCard[] SelectedGates;
        public int MinNumber = 0;
        public int MaxNumber = 7;

        public override bool HasValidTargets(Game game) { return game.GateIndex.Count(TargetValidator) >= MinNumber; }
    }

    class MultiBakuganSelector : Selector
    {
        public Func<Bakugan, bool> TargetValidator = (x) => true;
        public Bakugan[] SelectedBakugans;
        public int MinNumber = 0;
        public int MaxNumber = 7;

        public override bool HasValidTargets(Game game) { return game.BakuganIndex.Count(TargetValidator) >= MinNumber; }
    }

    class MultiAbilitySelector : Selector
    {
        public Func<AbilityCard, bool> TargetValidator = (x) => true;
        public AbilityCard[] SelectedAbilities;
        public int MinNumber = 0;
        public int MaxNumber = 7;

        public override bool HasValidTargets(Game game) { return game.AbilityIndex.Count(TargetValidator) >= MinNumber; }
    }

    class MultiActiveSelector : Selector
    {
        public Func<IActive, bool> TargetValidator = (x) => true;
        public IActive[] SelectedActives;
        public int MinNumber = 0;
        public int MaxNumber = 7;

        public override bool HasValidTargets(Game game) { return game.ActiveZone.Count(TargetValidator) >= MinNumber; }
    }

    class MultiGateSlotSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator = (x) => true;
        public (int X, int Y)[] SelectedSlots;
        public int MinNumber = 0;
        public int MaxNumber = 7;

        public override bool HasValidTargets(Game game) { return game.Field.Cast<GateCard?>().Count(x => x is null) > 4 + MinNumber; }
    }
}
