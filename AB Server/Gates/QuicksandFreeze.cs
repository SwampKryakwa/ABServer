using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server.Gates
{
    class QuicksandFreeze : GateCard
    {
        public override void DetermineWinner()
        {
            if (!Negated)
                Open();
            else
                base.DetermineWinner();
        }

        public override void Open()
        {
            base.Open();
        }

        public override bool IsOpenable() =>
            false;
    }
}
