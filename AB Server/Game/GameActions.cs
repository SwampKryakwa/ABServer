using AB_Server.Gates;

namespace AB_Server;

internal partial class Game
{
    //All the event types in the game
    public Action<(Bakugan, Boost)[]>? OnBakugansBoosted;
    public Action<(Bakugan, GateCard)[]>? OnBakugansMoved;
    public Action<Bakugan[]>? OnBakugansFromFieldToHands;
    public Action<Bakugan[]>? OnBakugansDestroyed;
    public Action<Bakugan[]>? OnBakugansFromDropToHands;
    public Action<(Bakugan, GateCard)[]>? OnBakugansFromHandsToField;
    public Action<(Bakugan, GateCard)[]>? OnBakugansFromDropToField;
    public Action<(Bakugan, AttributeState)[]>? OnBakugansAttributeChange;
    public Action<GateCard>? OnGateAdded;
    public Action<GateCard>? OnGateRemoved;
    public Action<GateCard>? OnGateOpen;
    public Action<GateCard>? OnBattleAboutToStart;
    public Action? OnBattlesStarted;
    public Action? OnBattlesOver;
    public Action? OnTurnStarted;
    public Action? OnTurnAboutToEnd;
    public Action? OnTurnEnd;

    public void OnSingleBakuganFromHandsToField(Bakugan bakugan, GateCard destination) =>
        OnBakugansFromHandsToField?.Invoke([(bakugan, destination)]);

    public void OnSingleBakuganMoved(Bakugan bakugan, GateCard destination) =>
        OnBakugansMoved?.Invoke([(bakugan, destination)]);

    public void OnSingleBakuganBoosted(Bakugan bakugan, Boost boost) =>
        OnBakugansBoosted?.Invoke([(bakugan, boost)]);

    public void OnSingleBakuganDestroyed(Bakugan bakugan) =>
        OnBakugansDestroyed?.Invoke([bakugan]);

    public void OnSingleBakuganAttributeChanged(Bakugan bakugan, AttributeState attributes) =>
        OnBakugansAttributeChange?.Invoke([(bakugan, attributes)]);

    public void OnSingleBakuganFromFieldToHand(Bakugan bakugan) =>
        OnBakugansFromFieldToHands?.Invoke([bakugan]);

    public void OnSingleBakuganFromDropToField(Bakugan bakugan, GateCard destination) =>
        OnBakugansFromDropToField?.Invoke([(bakugan, destination)]);

    public void OnSingleBakuganFromDropToHand(Bakugan bakugan) =>
        OnBakugansFromDropToHands?.Invoke([bakugan]);
}