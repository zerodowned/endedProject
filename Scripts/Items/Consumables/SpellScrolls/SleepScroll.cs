namespace Server.Items
{
    public class SleepScroll : SpellScroll
    {
        [Constructable]
        public SleepScroll()
            : this(1)
        {
        }

        [Constructable]
        public SleepScroll(int amount)
            : base(681, 0x2DA2, amount)
        {
        }

        public SleepScroll(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}