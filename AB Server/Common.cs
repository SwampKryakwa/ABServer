using AB_Server.Abilities;
using AB_Server.Gates;

namespace AB_Server;

internal interface IActive
{
    public int EffectId { get; set; }
    public int TypeId { get; }
    public CardKind Kind { get; }
    public Bakugan User { get; set; }

    public Player Owner { get; set; }

    public void Negate(bool asCounter = false);
}

enum ActivationWindow : byte
{
    Normal,
    TurnStart,
    TurnEnd,
    Intermediate
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

    public static bool IsAdjacent(IBakuganContainer pos1, IBakuganContainer pos2)
    {
        if (pos1 is GateCard gate1 && pos2 is GateCard gate2)
            return GateCard.AreAdjacent(gate1, gate2);
        return false;
    }
}

enum Attribute : byte
{
    Nova,
    Aqua,
    Darkon,
    Zephyros,
    Lumina,
    Subterra,
    Clear
}
enum Treatment : byte
{
    None,
    Flip,
    Pearl,
    Diamond,
    Translucent
}
enum BakuganType : sbyte
{
    None = -1,
    Glorius,
    Laserman,
    Mantis,
    Raptor,
    Lucifer,
    Saurus,
    Elephant,
    Tigress,
    Garrison,
    Griffon,
    Knight,
    Worm,
    Shredder,
    Fairy
}

enum MoveSource : byte
{
    Game,
    Effect
}
