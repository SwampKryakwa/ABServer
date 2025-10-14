using System.Runtime.CompilerServices;
using AB_Server.Gates;

namespace AB_Server.Abilities;

internal class FireWall(int cID, Player owner, int typeId) : AbilityCard(cID, owner, typeId)
{
    public override void TriggerEffect() =>
        new FireWallMarker(User, IsCopy).Activate();

    public override bool UserValidator(Bakugan user) =>
        user.OnField() && user.IsAttribute(Attribute.Nova);

    [ModuleInitializer]
    internal static void Init() => Register(9, CardKind.NormalAbility, (cID, owner) => new FireWall(cID, owner, 9));
}

internal class FireWallMarker(Bakugan user, bool isCopy) : IActive
{

    public int EffectId { get; set; } = user.Game.NextEffectId++;

    public int TypeId { get => 9; }

    public CardKind Kind { get => CardKind.NormalAbility; }

    public Bakugan User { get; set; } = user;
    public Player Owner { get; set; } = user.Owner;
    Game game = user.Game;

    public void Activate()
    {
        game.ActiveZone.Add(this);
        game.ThrowEvent(EventBuilder.AddMarkerToActiveZone(this, isCopy));

        game.OnBakugansFromHandsToField += OnBakuganToField;
        game.OnBakugansFromDropToField += OnBakuganToField;
        User.OnDestroyed += CeaseMarker;
    }

    private void OnBakuganToField((Bakugan, GateCard)[] additions)
    {
        for (int i = 0; i < additions.Length; i++)
            foreach (var oppBakugan in game.BakuganIndex.Where(x => User.IsOpponentOf(x) && x.Position is GateCard posGate && posGate.Owner != Owner))
                oppBakugan.Boost(new Boost(-50), this);
    }

    public void Negate(bool asCounter) =>
        CeaseMarker();

    private void CeaseMarker()
    {
        game.ActiveZone.Remove(this);
        game.ThrowEvent(EventBuilder.RemoveMarkerFromActiveZone(this));

        game.OnBakugansFromHandsToField -= OnBakuganToField;
        game.OnBakugansFromDropToField -= OnBakuganToField;
        User.OnDestroyed -= CeaseMarker;
    }
}