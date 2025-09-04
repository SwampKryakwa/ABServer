namespace AB_Server.Gates
{
    internal class DirectOpposition : GateCard
    {
        public DirectOpposition(int cID, Player owner)
        {
            game = owner.Game;
            Owner = owner;

            CardId = cID;

            CondTargetSelectors =
            [
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this },
                new BakuganSelector { ClientType = "BF", ForPlayer = x => x == Owner, Message = "INFO_GATE_TARGET", TargetValidator = x => x.Position == this && x != (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan }
            ];
        }

        public override int TypeId { get; } = 20;

        public override bool IsOpenable() =>
            game.CurrentWindow == ActivationWindow.Normal && OpenBlocking.Count == 0 && !IsOpen && !Negated && Bakugans.Count >= 2 && Bakugans.Any(x => x.Owner == Owner && x.InBattle);

        static readonly Attribute[] upperTriple = [Attribute.Aqua, Attribute.Nova, Attribute.Lumina];
        static readonly Attribute[] lowerTriple = [Attribute.Darkon, Attribute.Zephyros, Attribute.Subterra];
        public override void TriggerEffect()
        {
            Bakugan target1 = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            Bakugan target2 = (CondTargetSelectors[0] as BakuganSelector)!.SelectedBakugan;
            
            if (target1.Position != this || target2.Position != this) return;
            
            Attribute[] target1Attrs = (target1.attributeChanges.Count > 0) ? [.. target1.attributeChanges[^1].Attributes] : [target1.BaseAttribute];
            Attribute[] target2Attrs = (target2.attributeChanges.Count > 0) ? [.. target2.attributeChanges[^1].Attributes] : [target2.BaseAttribute];

            foreach (var attr1 in target1Attrs)
                foreach (var attr2 in target2Attrs)
                    if ((upperTriple.Contains(attr1) && upperTriple.Contains(attr2)) || (lowerTriple.Contains(attr1) && lowerTriple.Contains(attr2)))
                    {
                        var difference = target1.Power - target2.Power;
                        target1.Boost(-difference, this);
                        target2.Boost(difference, this);
                        return;
                    }
        }
    }
}
