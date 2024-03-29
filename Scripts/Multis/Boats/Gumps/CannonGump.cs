using Server.Items;
using Server.Network;

namespace Server.Gumps
{
    public class CannonGump : Gump
    {
        public static readonly int LabelColor = 0xFFFFFF;
        public static readonly int GreenHue = 5057;
        public static readonly int RedHue = 28776;

        private readonly BaseCannon m_Cannon;
        private readonly Mobile m_From;

        public BaseCannon Cannon => m_Cannon;

        public CannonGump(BaseCannon cannon, Mobile from)
            : base(50, 50)
        {
            m_Cannon = cannon;
            m_From = from;

            AddBackground(0, 0, 300, 200, 2620);

            bool charged = cannon.Charged;
            bool primed = cannon.Primed;
            bool loaded = cannon.AmmoType != AmmunitionType.Empty;

            AddHtmlLocalized(0, 10, 300, 16, 1149614 + (int)m_Cannon.Position, 21758, false, false);

            if (!charged)
                AddHtmlLocalized(45, 60, 100, 16, 1149630, LabelColor, false, false);  //CHARGE
            else
                AddHtmlLocalized(45, 60, 100, 16, 1149629, LabelColor, false, false);  //REMOVE

            if (!loaded)
                AddHtmlLocalized(45, 80, 100, 16, 1149635, LabelColor, false, false);  //LOAD
            else
                AddHtmlLocalized(45, 80, 100, 16, 1149629, LabelColor, false, false);  //REMOVE

            if (!primed)
                AddHtmlLocalized(45, 100, 100, 16, 1149637, LabelColor, false, false); //PRIME
            else if (cannon.CanLight)
                AddHtmlLocalized(45, 100, 100, 16, 1149638, LabelColor, false, false); //FIRE
            else
                AddHtmlLocalized(45, 100, 100, 16, 1149629, LabelColor, false, false);  //REMOVE

            if (!charged)
                AddHtmlLocalized(150, 60, 100, 16, 1149632, RedHue, false, false); //Not Charged
            else
                AddHtmlLocalized(150, 60, 100, 16, 1149631, GreenHue, false, false); //Charged

            if (!loaded)
                AddHtmlLocalized(150, 80, 100, 16, 1149636, RedHue, false, false); //Not Loaded
            else
                AddHtmlLocalized(150, 80, 100, 16, 1114057, AmmoInfo.GetAmmoName(cannon).ToString(), GreenHue, false, false);

            if (!primed)
                AddHtmlLocalized(150, 100, 100, 16, 1149639, RedHue, false, false); //No Fuse
            else
                AddHtmlLocalized(150, 100, 100, 16, 1149640, GreenHue, false, false); //Primed

            AddButton(10, 60, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
            AddButton(10, 80, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0);
            AddButton(10, 100, 0xFA5, 0xFA7, 3, GumpButtonType.Reply, 0);

            if (!cannon.Actions.ContainsKey(from) || cannon.Actions[from].Count == 0)
                cannon.AddAction(from, 1149653); //You are now operating the cannon.

            int y = 170;
            int count = cannon.Actions[from].Count - 1;
            int hue = 0;

            for (int i = count; i >= 0; i--)
            {
                if (i < count - 3)
                    break;

                if (i == count)
                    hue = 29315; //0xFFFF00;
                else hue = 12684;

                AddHtmlLocalized(10, y, 385, 20, cannon.Actions[from][i], hue, false, false);
                y -= 16;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (m_Cannon == null || m_Cannon.Deleted || !from.InRange(m_Cannon.Location, 3))
                return;

            switch (info.ButtonID)
            {
                default:
                case 0: return;
                case 1: //Charge
                    if (!m_Cannon.Charged)
                        m_Cannon.TryCharge(from);
                    else
                        m_Cannon.RemoveCharge(from);
                    break;
                case 2: //load
                    if (m_Cannon.AmmoType == AmmunitionType.Empty)
                        m_Cannon.TryLoad(from);
                    else
                        m_Cannon.RemoveLoad(from);
                    break;
                case 3: //prime
                    if (!m_Cannon.Primed)
                        m_Cannon.TryPrime(from);
                    else if (m_Cannon.CanLight)
                        m_Cannon.TryLightFuse(from);
                    else
                        m_Cannon.RemovePrime(from);
                    break;
            }

            from.SendGump(new CannonGump(m_Cannon, m_From));
        }
    }
}
