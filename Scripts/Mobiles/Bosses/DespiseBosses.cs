using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Despise
{
    public class DespiseBoss : BaseCreature
    {
        public static readonly int ArtifactChance = 5;

        public virtual BaseCreature SummonWisp => null;
        public virtual double WispScalar => 0.33;

        private BaseCreature m_Wisp;
        private Timer m_SummonTimer;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseCreature Wisp => m_Wisp;

        public DespiseBoss(AIType ai, FightMode fightmode) : base(ai, fightmode, 10, 1, .1, .2)
        {
            m_SummonTimer = Timer.DelayCall(TimeSpan.FromSeconds(5), SummonWisp_Callback);

            FollowersMax = 100;
        }

        public void SetNonMovable(Item item)
        {
            item.Movable = false;
            AddItem(item);
        }

        public override int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
        {
            if (from is DespiseCreature)
                return base.Damage(amount, from, informMount, checkDisrupt);

            return 0;
        }

        public override bool OnBeforeDeath()
        {
            if (m_Wisp != null && !m_Wisp.Deleted)
            {
                m_Wisp.Delete();
                m_SummonTimer = null;
            }

            return base.OnBeforeDeath();
        }

        public override void OnKilledBy(Mobile mob)
        {
            if (mob is PlayerMobile mobile)
            {
                int chance = ArtifactChance + Math.Min(10, mobile.Luck / 180);

                if (chance >= Utility.Random(100))
                {
                    Type t = m_Artifacts[Utility.Random(m_Artifacts.Length)];

                    if (t != null)
                    {
                        Item arty = Loot.Construct(t);

                        if (arty != null)
                        {
                            Container pack = mobile.Backpack;

                            if (pack == null || !pack.TryDropItem(mobile, arty, false))
                            {
                                mobile.BankBox.DropItem(arty);
                                mobile.SendMessage("An artifact has been placed in your bankbox!");
                            }
                            else
                                mobile.SendLocalizedMessage(1153440); // An artifact has been placed in your backpack!
                        }
                    }
                }
            }

            base.OnKilledBy(mob);
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            base.AlterMeleeDamageTo(to, ref damage);

            if (m_Wisp != null && !m_Wisp.Deleted && m_Wisp.Alive)
                damage += (int)(damage * WispScalar);
        }

        public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
        {
            base.AlterMeleeDamageFrom(from, ref damage);

            if (m_Wisp != null && !m_Wisp.Deleted && m_Wisp.Alive)
                damage -= (int)(damage * WispScalar);
        }

        public override void AlterSpellDamageTo(Mobile to, ref int damage)
        {
            base.AlterSpellDamageTo(to, ref damage);

            if (m_Wisp != null && !m_Wisp.Deleted && m_Wisp.Alive)
                damage += (int)(damage * WispScalar);
        }

        public override void AlterSpellDamageFrom(Mobile from, ref int damage)
        {
            base.AlterSpellDamageFrom(from, ref damage);

            if (m_Wisp != null && !m_Wisp.Deleted && m_Wisp.Alive)
                damage -= (int)(damage * WispScalar);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (m_SummonTimer == null && (m_Wisp == null || !m_Wisp.Alive || m_Wisp.Deleted))
            {
                m_SummonTimer = Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(40, 60)), SummonWisp_Callback);
            }
        }

        public void SummonWisp_Callback()
        {
            m_Wisp = SummonWisp;
            m_Wisp.MoveToWorld(Location, Map);
            m_SummonTimer = null;
        }

        public override void Delete()
        {
            base.Delete();

            if (m_Wisp != null && m_Wisp.Alive)
                m_Wisp.Delete();
        }

        public static Type[] Artifacts => m_Artifacts;

        private static readonly Type[] m_Artifacts =
        {
            typeof(CompassionsEye),
            typeof(UnicornManeWovenSandals),
            typeof(UnicornManeWovenTalons),
            typeof(DespicableQuiver),
            typeof(UnforgivenVeil),
            typeof(HailstormHuman),
            typeof(HailstormGargoyle)
        };

        public DespiseBoss(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Wisp);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Wisp = reader.ReadMobile() as BaseCreature;
        }
    }

    public class AdrianTheGloriousLord : DespiseBoss
    {
        [Constructable]
        public AdrianTheGloriousLord() : base(AIType.AI_Mage, FightMode.Closest)
        {
            Name = "Adrian";
            Title = "the Glorious Lord";

            Race = Race.Human;
            Body = 0x190;
            Female = false;

            Hue = Race.RandomSkinHue();
            HairItemID = 8252;
            HairHue = 153;

            SetStr(900, 1200);
            SetDex(500, 600);
            SetInt(500, 600);

            SetHits(60000);
            SetStam(415);
            SetMana(22000);

            SetDamage(18, 28);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 60);
            SetResistance(ResistanceType.Fire, 40, 60);
            SetResistance(ResistanceType.Cold, 40, 60);
            SetResistance(ResistanceType.Poison, 40, 60);
            SetResistance(ResistanceType.Energy, 40, 60);

            SetSkill(SkillName.MagicResist, 120);
            SetSkill(SkillName.Tactics, 120);
            SetSkill(SkillName.Wrestling, 120);
            SetSkill(SkillName.Anatomy, 120);
            SetSkill(SkillName.Magery, 120);
            SetSkill(SkillName.EvalInt, 120);
            SetSkill(SkillName.Mysticism, 120);
            SetSkill(SkillName.Focus, 160);

            Fame = 22000;
            Karma = 22000;

            Item boots = new ThighBoots
            {
                Hue = 1
            };

            Item scimitar = new Item(5046)
            {
                Hue = 1818,
                Layer = Layer.OneHanded
            };

            SetNonMovable(boots);
            SetNonMovable(scimitar);
            SetNonMovable(new LongPants(1818));
            SetNonMovable(new FancyShirt(194));
            SetNonMovable(new Doublet(1281));
        }

        public override bool InitialInnocent => true;
        public override BaseCreature SummonWisp => new EnsorcledWisp(this);

        public override void GenerateLoot()
        {
            AddLoot(LootPack.SuperBoss, 3);
        }

        public AdrianTheGloriousLord(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class AndrosTheDreadLord : DespiseBoss
    {
        [Constructable]
        public AndrosTheDreadLord() : base(AIType.AI_Mage, FightMode.Closest)
        {
            Name = "Andros";
            Title = "the Dread Lord";

            Race = Race.Human;
            Body = 0x190;
            Female = false;

            Hue = Race.RandomSkinHue();
            HairItemID = 0;

            SetStr(900, 1200);
            SetDex(500, 600);
            SetInt(500, 600);

            SetHits(60000);
            SetStam(415);
            SetMana(22000);

            SetDamage(18, 28);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 60);
            SetResistance(ResistanceType.Fire, 40, 60);
            SetResistance(ResistanceType.Cold, 40, 60);
            SetResistance(ResistanceType.Poison, 40, 60);
            SetResistance(ResistanceType.Energy, 40, 60);

            SetSkill(SkillName.MagicResist, 120);
            SetSkill(SkillName.Tactics, 120);
            SetSkill(SkillName.Wrestling, 120);
            SetSkill(SkillName.Anatomy, 120);
            SetSkill(SkillName.Magery, 120);
            SetSkill(SkillName.EvalInt, 120);
            SetSkill(SkillName.Mysticism, 120);
            SetSkill(SkillName.Focus, 160);

            Fame = 22000;
            Karma = -22000;

            Item boots = new ThighBoots
            {
                Hue = 1
            };

            Item staff = new Item(3721)
            {
                Layer = Layer.TwoHanded
            };

            SetNonMovable(boots);
            SetNonMovable(new LongPants(1818));
            SetNonMovable(new FancyShirt(2726));
            SetNonMovable(new Doublet(1153));
            SetNonMovable(staff);
        }

        public override bool AlwaysMurderer => true;
        public override BaseCreature SummonWisp => new CorruptedWisp(this);

        public override void GenerateLoot()
        {
            AddLoot(LootPack.SuperBoss, 3);
        }

        public AndrosTheDreadLord(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class EnsorcledWisp : BaseCreature
    {
        public Mobile Boss { get; set; }

        public EnsorcledWisp(Mobile boss)
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
        {
            Boss = boss;
            Name = "Ensorcled Wisp";
            Body = 0x3A;
            Hue = 1927;
            BaseSoundID = 466;

            SetStr(600, 700);
            SetDex(500, 600);
            SetInt(500, 600);

            SetHits(7000, 8000);

            SetDamage(12, 19);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Fire, 30);
            SetDamageType(ResistanceType.Energy, 30);

            SetResistance(ResistanceType.Physical, 50, 65);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 50, 65);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.MagicResist, 110, 120);
            SetSkill(SkillName.Tactics, 110, 120);
            SetSkill(SkillName.Wrestling, 110, 120);
            SetSkill(SkillName.Anatomy, 110, 120);
            SetSkill(SkillName.Focus, 110, 120);
            SetSkill(SkillName.EvalInt, 110, 120);
            SetSkill(SkillName.Magery, 110, 120);
            SetSkill(SkillName.Parry, 110, 120);

            Fame = 8000;
            Karma = 8000;
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Boss != null && !Boss.Deleted && !InRange(Boss, 20))
            {
                MoveToWorld(Boss.Location, Boss.Map);
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
        }

        public override bool InitialInnocent => true;

        public EnsorcledWisp(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Boss);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Boss = reader.ReadMobile();
        }
    }

    public class CorruptedWisp : BaseCreature
    {
        public Mobile Boss { get; set; }

        public CorruptedWisp(Mobile boss)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Boss = boss;
            Name = "Corrupted Wisp";
            Body = 0xA5;
            Hue = 1964;
            BaseSoundID = 466;

            SetStr(600, 700);
            SetDex(500, 600);
            SetInt(500, 600);

            SetHits(7000, 8000);

            SetDamage(12, 19);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Fire, 30);
            SetDamageType(ResistanceType.Energy, 30);

            SetResistance(ResistanceType.Physical, 50, 65);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 50, 65);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.MagicResist, 110, 120);
            SetSkill(SkillName.Tactics, 110, 120);
            SetSkill(SkillName.Wrestling, 110, 120);
            SetSkill(SkillName.Anatomy, 110, 120);
            SetSkill(SkillName.Focus, 110, 120);
            SetSkill(SkillName.EvalInt, 110, 120);
            SetSkill(SkillName.Magery, 110, 120);
            SetSkill(SkillName.Parry, 110, 120);

            Fame = 8000;
            Karma = -8000;
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Boss != null && !Boss.Deleted && !InRange(Boss, 20))
            {
                MoveToWorld(Boss.Location, Boss.Map);
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
        }

        public override bool AlwaysMurderer => true;

        public CorruptedWisp(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Boss);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Boss = reader.ReadMobile();
        }
    }
}
