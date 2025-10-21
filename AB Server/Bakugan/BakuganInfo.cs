using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_Server;

internal partial class Bakugan
{
    //Relations
    public Game Game = game;
    public Player Owner = owner;

    //Bakugan info
    public int BID = BID;
    public BakuganType Type = type;
    public Treatment Treatment = treatment;
    public Attribute BaseAttribute = attribute;
    public bool IsPartner = false;
    public short DefaultPower { get; } = power;
    public short BasePower = power;

    //Extra info
    public bool IsDummy = false;
}
