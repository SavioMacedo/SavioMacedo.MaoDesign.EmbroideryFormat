using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Jef
{
    public class JefThread : EmbThread
    {
        private JefThread(uint color, string description, string catalogNumber) : base(color: color, description: description, catalogNumber: catalogNumber, brand: "Jef", chart: "Jef")
        {
        }

        public static JefThread[] GetThreadSet()
        {
            return new[]
            {
                 new JefThread(0x000000, "Placeholder", "000"),
                 new JefThread(0x000000, "Black", "002"),
                 new JefThread(0xffffff, "White", "001"),
                 new JefThread(0xffff17, "Yellow", "204"),
                 new JefThread(0xff6600, "Orange", "203"),
                 new JefThread(0x2f5933, "Olive Green", "219"),
                 new JefThread(0x237336, "Green", "226"),
                 new JefThread(0x65c2c8, "Sky", "217"),
                 new JefThread(0xab5a96, "Purple", "208"),
                 new JefThread(0xf669a0, "Pink", "201"),
                 new JefThread(0xff0000, "Red", "225"),
                 new JefThread(0xb1704e, "Brown", "214"),
                 new JefThread(0x0b2f84, "Blue", "207"),
                 new JefThread(0xe4c35d, "Gold", "003"),
                 new JefThread(0x481a05, "Dark Brown", "205"),
                 new JefThread(0xac9cc7, "Pale Violet", "209"),
                 new JefThread(0xfcf294, "Pale Yellow", "210"),
                 new JefThread(0xf999b7, "Pale Pink", "211"),
                 new JefThread(0xfab381, "Peach", "212"),
                 new JefThread(0xc9a480, "Beige", "213"),
                 new JefThread(0x970533, "Wine Red", "215"),
                 new JefThread(0xa0b8cc, "Pale Sky", "216"),
                 new JefThread(0x7fc21c, "Yellow Green", "218"),
                 new JefThread(0xe5e5e5, "Silver Gray", "220"),
                 new JefThread(0x889b9b, "Gray", "221"),
                 new JefThread(0x98d6bd, "Pale Aqua", "227"),
                 new JefThread(0xb2e1e3, "Baby Blue", "228"),
                 new JefThread(0x368ba0, "Powder Blue", "229"),
                 new JefThread(0x4f83ab, "Bright Blue", "230"),
                 new JefThread(0x386a91, "Slate Blue", "231"),
                 new JefThread(0x071650, "Navy Blue", "232"),
                 new JefThread(0xf999a2, "Salmon Pink", "233"),
                 new JefThread(0xf9676b, "Coral", "234"),
                 new JefThread(0xe3311f, "Burnt Orange", "235"),
                 new JefThread(0xe2a188, "Cinnamon", "236"),
                 new JefThread(0xb59474, "Umber", "237"),
                 new JefThread(0xe4cf99, "Blond", "238"),
                 new JefThread(0xffcb00, "Sunflower", "239"),
                 new JefThread(0xe1add4, "Orchid Pink", "240"),
                 new JefThread(0xc3007e, "Peony Purple", "241"),
                 new JefThread(0x80004b, "Burgundy", "242"),
                 new JefThread(0x540571, "Royal Purple", "243"),
                 new JefThread(0xb10525, "Cardinal Red", "244"),
                 new JefThread(0xcae0c0, "Opal Green", "245"),
                 new JefThread(0x899856, "Moss Green", "246"),
                 new JefThread(0x5c941a, "Meadow Green", "247"),
                 new JefThread(0x003114, "Dark Green", "248"),
                 new JefThread(0x5dae94, "Aquamarine", "249"),
                 new JefThread(0x4cbf8f, "Emerald Green", "250"),
                 new JefThread(0x007772, "Peacock Green", "251"),
                 new JefThread(0x595b61, "Dark Gray", "252"),
                 new JefThread(0xfffff2, "Ivory White", "253"),
                 new JefThread(0xb15818, "Hazel", "254"),
                 new JefThread(0xcb8a07, "Toast", "255"),
                 new JefThread(0x986c80, "Salmon", "256"),
                 new JefThread(0x98692d, "Cocoa Brown", "257"),
                 new JefThread(0x4d3419, "Sienna", "258"),
                 new JefThread(0x4c330b, "Sepia", "259"),
                 new JefThread(0x33200a, "Dark Sepia", "260"),
                 new JefThread(0x523a97, "Violet Blue", "261"),
                 new JefThread(0x0d217e, "Blue Ink", "262"),
                 new JefThread(0x1e77ac, "Sola Blue", "263"),
                 new JefThread(0xb2dd53, "Green Dust", "264"),
                 new JefThread(0xf33689, "Crimson", "265"),
                 new JefThread(0xde649e, "Floral Pink", "266"),
                 new JefThread(0x984161, "Wine", "267"),
                 new JefThread(0x4c5612, "Olive Drab", "268"),
                 new JefThread(0x4c881f, "Meadow", "269"),
                 new JefThread(0xe4de79, "Mustard", "270"),
                 new JefThread(0xcb8a1a, "Yellow Ocher", "271"),
                 new JefThread(0xcba21c, "Old Gold", "272"),
                 new JefThread(0xff9805, "Honey Dew", "273"),
                 new JefThread(0xfcb257, "Tangerine", "274"),
                 new JefThread(0xffe505, "Canary Yellow", "275"),
                 new JefThread(0xf0331f, "Vermilion", "202"),
                 new JefThread(0x1a842d, "Bright Green", "206"),
                 new JefThread(0x386cae, "Ocean Blue", "222"),
                 new JefThread(0xe3c4b4, "Beige Gray", "223"),
                 new JefThread(0xe3ac81, "Bamboo", "224")
            };
        }
    }
}
