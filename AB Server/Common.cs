using AB_Server.Abilities;
using AB_Server.Gates;

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

    class Selector
    {
        public string ClientType;
        public string Message;
        public int ForPlayer;
        public Func<bool> Condition = () => true;
    }

    class OptionSelector : Selector
    {
        public int OptionCount;
        public int SelectedOption;
    }

    class GateSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator;
        public GateCard SelectedGate;
    }

    class BakuganSelector : Selector
    {
        public Func<Bakugan, bool> TargetValidator;
        public Bakugan SelectedBakugan;
    }

    class AbilitySelector : Selector
    {
        public Func<AbilityCard, bool> TargetValidator;
        public AbilityCard SelectedAbility;
    }

    class ActiveSelector : Selector
    {
        public Func<IActive, bool> TragetValidator;
        public IActive SelectedActive;
    }

    class MultiGateSelector : Selector
    {
        public Func<GateCard, bool> TargetValidator;
        public GateCard[] SelectedGates;
    }

    class MultiBakuganSelector : Selector
    {
        public Func<Bakugan, bool> TargetValidator;
        public Bakugan[] SelectedBakugans;
    }

    class MultiAbilitySelector : Selector
    {
        public Func<AbilityCard, bool> TargetValidator;
        public AbilityCard[] SelectedAbilities;
    }

    class MultiActiveSelector : Selector
    {
        public Func<IActive, bool> TragetValidator;
        public IActive[] SelectedActives;
    }
}
