using AB_Server.Gates;

namespace AB_Server;

class Boost(short value)
{
    public short Value { get; set; } = value;
    public bool Active = true;
}

class AttributeState(params Attribute[] attributes)
{
    public Attribute[] Attributes = attributes;

    public bool IsAttribute(Attribute attribute) =>
        Attributes.Contains(attribute);

    public void AddAttribute(Attribute attribute)
    {
        Attributes = [.. Attributes, attribute];
    }
}

internal partial class Bakugan(BakuganType type, short power, Attribute attribute, Treatment treatment, Player owner, Game game, int BID)
{

    public static Bakugan GetDummy() => new(BakuganType.None, 0, Attribute.Clear, Treatment.None, null, null, -1)
    {
        IsDummy = true
    };

    public bool IsOpponentOf(Bakugan bakugan) =>
        Owner.TeamId != bakugan.Owner.TeamId;

    public bool HasNeighbourEnemies()
    {
        if (Position is GateCard positionGate)
        {
            if (!OnField() ||
                !Game.BakuganIndex.Any(x => IsOpponentOf(x) && x.OnField())) return false;
            (int X, int Y) = positionGate.Position;

            if (Game.GetGateByCoord(X - 1, Y) is GateCard gate1 && gate1.Bakugans.Any(IsOpponentOf)) return true;
            if (Game.GetGateByCoord(X + 1, Y) is GateCard gate2 && gate2.Bakugans.Any(IsOpponentOf)) return true;
            if (Game.GetGateByCoord(X, Y - 1) is GateCard gate3 && gate3.Bakugans.Any(IsOpponentOf)) return true;
            if (Game.GetGateByCoord(X, Y + 1) is GateCard gate4 && gate4.Bakugans.Any(IsOpponentOf)) return true;
        }
        return false;
    }

    public static bool IsAdjacent(Bakugan bakugan1, Bakugan bakugan2)
    {

        List<Attribute> attrs1 = bakugan1.attributeChanges.Count == 0 ? [bakugan1.BaseAttribute] : [.. bakugan1.attributeChanges[^1].Attributes];
        List<Attribute> attrs2 = bakugan2.attributeChanges.Count == 0 ? [bakugan2.BaseAttribute] : [.. bakugan2.attributeChanges[^1].Attributes];

        return attrs1.Contains(Attribute.Nova) && attrs2.Contains(Attribute.Subterra) ||
            attrs1.Contains(Attribute.Subterra) && attrs2.Contains(Attribute.Lumina) ||
            attrs1.Contains(Attribute.Lumina) && attrs2.Contains(Attribute.Darkon) ||
            attrs1.Contains(Attribute.Darkon) && attrs2.Contains(Attribute.Aqua) ||
            attrs1.Contains(Attribute.Aqua) && attrs2.Contains(Attribute.Zephyros) ||
            attrs1.Contains(Attribute.Zephyros) && attrs2.Contains(Attribute.Nova);
    }

    public static bool IsDiagonal(Bakugan bakugan1, Bakugan bakugan2)
    {

        List<Attribute> attrs1 = bakugan1.attributeChanges.Count == 0 ? [bakugan1.BaseAttribute] : [.. bakugan1.attributeChanges[^1].Attributes];
        List<Attribute> attrs2 = bakugan2.attributeChanges.Count == 0 ? [bakugan2.BaseAttribute] : [.. bakugan2.attributeChanges[^1].Attributes];

        return attrs1.Contains(Attribute.Nova) && attrs2.Contains(Attribute.Darkon) ||
            attrs1.Contains(Attribute.Darkon) && attrs2.Contains(Attribute.Nova) ||
            attrs1.Contains(Attribute.Subterra) && attrs2.Contains(Attribute.Aqua) ||
            attrs1.Contains(Attribute.Aqua) && attrs2.Contains(Attribute.Subterra) ||
            attrs1.Contains(Attribute.Lumina) && attrs2.Contains(Attribute.Zephyros) ||
            attrs1.Contains(Attribute.Zephyros) && attrs2.Contains(Attribute.Lumina);
    }

    public static bool IsTripleNode(out bool isPositive, params IEnumerable<Bakugan> bakugans)
    {
        List<Attribute> attrs = [];
        foreach (Bakugan b in bakugans)
            if (b.attributeChanges.Count == 0)
                attrs.Add(b.BaseAttribute);
            else
                attrs.AddRange(b.attributeChanges[^1].Attributes);

        isPositive = false;
        if (attrs.Contains(Attribute.Aqua) && attrs.Contains(Attribute.Nova) && attrs.Contains(Attribute.Lumina))
        {
            isPositive = true;
            return true;
        }
        else if (attrs.Contains(Attribute.Subterra) && attrs.Contains(Attribute.Zephyros) && attrs.Contains(Attribute.Darkon))
            return true;
        else
            return false;
    }
}
