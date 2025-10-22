using Newtonsoft.Json.Linq;

namespace AB_Server;

internal partial class Bakugan
{
    public void Boost(int boost, object source)
    {
        Boost(new Boost((short)boost), source);
    }

    public void Boost(Boost boost, object source)
    {
        if (IsDummy || InHand()) return;

        Boosts.Add(boost);

        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganBoostedEvent",
            ["Owner"] = Owner.PlayerId,
            ["Boost"] = boost.Value,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["InHand"] = InHand(),
                ["InGrave"] = InDrop(),
                ["BID"] = BID
            }
        });

        Game.OnSingleBakuganBoosted(this, boost);
    }

    public void ContinuousBoost(Boost boost, object source)
    {
        if (IsDummy) return;

        ContinuousBoosts.Add(boost);

        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganBoostedEvent",
            ["Owner"] = Owner.PlayerId,
            ["Boost"] = boost.Value,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["InHand"] = InHand(),
                ["InGrave"] = InDrop(),
                ["BID"] = BID
            }
        });

        Game.OnSingleBakuganBoosted(this, boost);
    }

    public void RemoveBoost(Boost boost, object source)
    {
        if (IsDummy) return;

        Boosts.Remove(boost);
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganBoostedEvent",
            ["Owner"] = Owner.PlayerId,
            ["Boost"] = -boost.Value,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["InHand"] = InHand(),
                ["InGrave"] = InDrop(),
                ["BID"] = BID
            }
        });
    }

    public void RemoveContinuousBoost(Boost boost, object source)
    {
        if (IsDummy) return;

        ContinuousBoosts.Remove(boost);
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganBoostedEvent",
            ["Owner"] = Owner.PlayerId,
            ["Boost"] = -boost.Value,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["BasePower"] = BasePower,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            }
        });
    }

    public AttributeState ChangeAttribute(Attribute newAttribute, object source)
    {
        Attribute oldAttribute = attributeChanges.Count == 0 ? BaseAttribute : attributeChanges[^1].Attributes[0];
        AttributeState change = new(newAttribute);
        attributeChanges.Add(change);

        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganAttributeChangeEvent",
            ["Owner"] = Owner.PlayerId,
            ["OldAttribute"] = (int)oldAttribute,
            ["Attribute"] = (int)newAttribute,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            }
        });

        Game.OnSingleBakuganAttributeChanged(this, change);

        return change;
    }

    public void RevertAttributeChange(AttributeState change, object source)
    {
        attributeChanges.Remove(change);

        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganAttributeChangeEvent",
            ["Owner"] = Owner.PlayerId,
            ["OldAttribute"] = (int)change.Attributes[0],
            ["Attribute"] = (int)(attributeChanges.Count == 0 ? BaseAttribute : attributeChanges[^1].Attributes[0]),
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            }
        });
    }

    public void TurnFrenzied()
    {
        if (!OnField()) return;
        Frenzied = true;
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganFrenzy",
            ["Owner"] = Owner.PlayerId,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            }
        });
    }

    public void StopFrenzy()
    {
        if (!OnField()) return;
        Frenzied = false;
        Game.ThrowEvent(new JObject
        {
            ["Type"] = "BakuganUnfrenzied",
            ["Owner"] = Owner.PlayerId,
            ["Bakugan"] = new JObject
            {
                ["Type"] = (int)Type,
                ["Attribute"] = (int)BaseAttribute,
                ["Treatment"] = (int)Treatment,
                ["Power"] = Power,
                ["IsPartner"] = IsPartner,
                ["BID"] = BID
            }
        });
    }
}
