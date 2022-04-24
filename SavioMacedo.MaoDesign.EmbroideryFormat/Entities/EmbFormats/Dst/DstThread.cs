using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Dst
{
    public class DstThread : EmbThread
    {
        public DstThread(string hexColor, string name, string colorNumber) : base(description: name, catalogNumber: colorNumber, brand: "DST", chart: "DST")
        {
            SetColor(hexColor);
        }

        private DstThread(byte red, byte green, byte blue, string name, string colorNumber) : base(description: name, catalogNumber: colorNumber, brand: "DST", chart: "DST")
        {
            SetColor(red, green, blue);
        }

        public static DstThread[] GetThreadSet()
        {
            return new[]
            {
                new DstThread(0, 0, 0, "Unknown", "0"),
                new DstThread(0, 0, 0, "Black", "1"),
                new DstThread(255, 255, 255, "White", "2"),
                new DstThread(255, 255, 23, "Sunflower", "3"),
                new DstThread(250, 160, 96, "Hazel", "4"),
                new DstThread(92, 118, 73, "Olive Green", "5"),
                new DstThread(64, 192, 48, "Green", "6"),
                new DstThread(101, 194, 200, "Sky", "7"),
                new DstThread(172, 128, 190, "Purple", "8"),
                new DstThread(245, 188, 203, "Pink", "9"),
                new DstThread(255, 0, 0, "Red", "10"),
                new DstThread(192, 128, 0, "Brown", "11"),
                new DstThread(0, 0, 240, "Blue", "12"),
                new DstThread(228, 195, 93, "Gold", "13"),
                new DstThread(165, 42, 42, "Dark Brown", "14"),
                new DstThread(213, 176, 212, "Pale Violet", "15"),
                new DstThread(252, 242, 148, "Pale Yellow", "16"),
                new DstThread(240, 208, 192, "Pale Pink", "17"),
                new DstThread(255, 192, 0, "Peach", "18"),
                new DstThread(201, 164, 128, "Beige", "19"),
                new DstThread(155, 61, 75, "Wine Red", "20"),
                new DstThread(160, 184, 204, "Pale Sky", "21"),
                new DstThread(127, 194, 28, "Yellow Green", "22"),
                new DstThread(185, 185, 185, "Silver Grey", "23"),
                new DstThread(160, 160, 160, "Grey", "24"),
                new DstThread(152, 214, 189, "Pale Aqua", "25"),
                new DstThread(184, 240, 240, "Baby Blue", "26"),
                new DstThread(54, 139, 160, "Powder Blue", "27"),
                new DstThread(79, 131, 171, "Bright Blue", "28"),
                new DstThread(56, 106, 145, "Slate Blue", "29"),
                new DstThread(0, 32, 107, "Nave Blue", "30"),
                new DstThread(229, 197, 202, "Salmon Pink", "31"),
                new DstThread(249, 103, 107, "Coral", "32"),
                new DstThread(227, 49, 31, "Burnt Orange", "33"),
                new DstThread(226, 161, 136, "Cinnamon", "34"),
                new DstThread(181, 148, 116, "Umber", "35"),
                new DstThread(228, 207, 153, "Blonde", "36"),
                new DstThread(225, 203, 0, "Sunflower", "37"),
                new DstThread(225, 173, 212, "Orchid Pink", "38"),
                new DstThread(195, 0, 126, "Peony Purple", "39"),
                new DstThread(128, 0, 75, "Burgundy", "40"),
                new DstThread(160, 96, 176, "Royal Purple", "41"),
                new DstThread(192, 64, 32, "Cardinal Red", "42"),
                new DstThread(202, 224, 192, "Opal Green", "43"),
                new DstThread(137, 152, 86, "Moss Green", "44"),
                new DstThread(0, 170, 0, "Meadow Green", "45"),
                new DstThread(33, 138, 33, "Dark Green", "46"),
                new DstThread(93, 174, 148, "Aquamarine", "47"),
                new DstThread(76, 191, 143, "Emerald Green", "48"),
                new DstThread(0, 119, 114, "Peacock Green", "49"),
                new DstThread(112, 112, 112, "Dark Grey", "50"),
                new DstThread(242, 255, 255, "Ivory White", "51"),
                new DstThread(177, 88, 24, "Hazel", "52"),
                new DstThread(203, 138, 7, "Toast", "53"),
                new DstThread(247, 146, 123, "Salmon", "54"),
                new DstThread(152, 105, 45, "Cocoa Brown", "55"),
                new DstThread(162, 113, 72, "Sienna", "56"),
                new DstThread(123, 85, 74, "Sepia", "57"),
                new DstThread(79, 57, 70, "Dark Sepia", "58"),
                new DstThread(82, 58, 151, "Violet Blue", "59"),
                new DstThread(0, 0, 160, "Blue Ink", "60"),
                new DstThread(0, 150, 222, "Solar Blue", "61"),
                new DstThread(178, 221, 83, "Green Dust", "62"),
                new DstThread(250, 143, 187, "Crimson", "63"),
                new DstThread(222, 100, 158, "Floral Pink", "64"),
                new DstThread(181, 80, 102, "Wine", "65"),
                new DstThread(94, 87, 71, "Olive Drab", "66"),
                new DstThread(76, 136, 31, "Meadow", "67"),
                new DstThread(228, 220, 121, "Canary Yellow", "68"),
                new DstThread(203, 138, 26, "Toast", "69"),
                new DstThread(198, 170, 66, "Beige", "70"),
                new DstThread(236, 176, 44, "Honey Dew", "71"),
                new DstThread(248, 128, 64, "Tangerine", "72"),
                new DstThread(255, 229, 5, "Ocean Blue", "73"),
                new DstThread(250, 122, 122, "Sepia", "74"),
                new DstThread(107, 224, 0, "Royal Purple", "75"),
                new DstThread(56, 108, 174, "Yellow Ocher", "76"),
                new DstThread(208, 186, 176, "Beige Grey", "77"),
                new DstThread(227, 190, 129, "Bamboo", "78")
            };
        }
    }
}
