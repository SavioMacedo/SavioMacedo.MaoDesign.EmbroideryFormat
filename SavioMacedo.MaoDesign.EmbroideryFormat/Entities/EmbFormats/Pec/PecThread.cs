using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Pec
{
    public class PecThread : EmbThread
    {
        public PecThread()
        {
        }

        private PecThread(uint red, uint green, uint blue, string description, string catalogNumber) : base(description: description, catalogNumber: catalogNumber, brand: "Pes", chart: "Pes")
        {
            SetColor(red, green, blue);
        }

        public static PecThread[] GetThreadSet()
        {
            return new[] {
                        new PecThread(0, 0, 0, "Unknown", "0"),
                        new PecThread(14, 31, 124, "Prussian Blue", "1"),
                        new PecThread(10, 85, 163, "Blue", "2"),
                        new PecThread(0, 135, 119, "Teal Green", "3"),
                        new PecThread(75, 107, 175, "Cornflower Blue", "4"),
                        new PecThread(237, 23, 31, "Red", "5"),
                        new PecThread(209, 92, 0, "Reddish Brown", "6"),
                        new PecThread(145, 54, 151, "Magenta", "7"),
                        new PecThread(228, 154, 203, "Light Lilac", "8"),
                        new PecThread(145, 95, 172, "Lilac", "9"),
                        new PecThread(158, 214, 125, "Mint Green", "10"),
                        new PecThread(232, 169, 0, "Deep Gold", "11"),
                        new PecThread(254, 186, 53, "Orange", "12"),
                        new PecThread(255, 255, 0, "Yellow", "13"),
                        new PecThread(112, 188, 31, "Lime Green", "14"),
                        new PecThread(186, 152, 0, "Brass", "15"),
                        new PecThread(168, 168, 168, "Silver", "16"),
                        new PecThread(125, 111, 0, "Russet Brown", "17"),
                        new PecThread(255, 255, 179, "Cream Brown", "18"),
                        new PecThread(79, 85, 86, "Pewter", "19"),
                        new PecThread(0, 0, 0, "Black", "20"),
                        new PecThread(11, 61, 145, "Ultramarine", "21"),
                        new PecThread(119, 1, 118, "Royal Purple", "22"),
                        new PecThread(41, 49, 51, "Dark Gray", "23"),
                        new PecThread(42, 19, 1, "Dark Brown", "24"),
                        new PecThread(246, 74, 138, "Deep Rose", "25"),
                        new PecThread(178, 118, 36, "Light Brown", "26"),
                        new PecThread(252, 187, 197, "Salmon Pink", "27"),
                        new PecThread(254, 55, 15, "Vermilion", "28"),
                        new PecThread(240, 240, 240, "White", "29"),
                        new PecThread(106, 28, 138, "Violet", "30"),
                        new PecThread(168, 221, 196, "Seacrest", "31"),
                        new PecThread(37, 132, 187, "Sky Blue", "32"),
                        new PecThread(254, 179, 67, "Pumpkin", "33"),
                        new PecThread(255, 243, 107, "Cream Yellow", "34"),
                        new PecThread(208, 166, 96, "Khaki", "35"),
                        new PecThread(209, 84, 0, "Clay Brown", "36"),
                        new PecThread(102, 186, 73, "Leaf Green", "37"),
                        new PecThread(19, 74, 70, "Peacock Blue", "38"),
                        new PecThread(135, 135, 135, "Gray", "39"),
                        new PecThread(216, 204, 198, "Warm Gray", "40"),
                        new PecThread(67, 86, 7, "Dark Olive", "41"),
                        new PecThread(253, 217, 222, "Flesh Pink", "42"),
                        new PecThread(249, 147, 188, "Pink", "43"),
                        new PecThread(0, 56, 34, "Deep Green", "44"),
                        new PecThread(178, 175, 212, "Lavender", "45"),
                        new PecThread(104, 106, 176, "Wisteria Violet", "46"),
                        new PecThread(239, 227, 185, "Beige", "47"),
                        new PecThread(247, 56, 102, "Carmine", "48"),
                        new PecThread(181, 75, 100, "Amber Red", "49"),
                        new PecThread(19, 43, 26, "Olive Green", "50"),
                        new PecThread(199, 1, 86, "Dark Fuchsia", "51"),
                        new PecThread(254, 158, 50, "Tangerine", "52"),
                        new PecThread(168, 222, 235, "Light Blue", "53"),
                        new PecThread(0, 103, 62, "Emerald Green", "54"),
                        new PecThread(78, 41, 144, "Purple", "55"),
                        new PecThread(47, 126, 32, "Moss Green", "56"),
                        new PecThread(255, 204, 204, "Flesh Pink", "57"),
                        new PecThread(255, 217, 17, "Harvest Gold", "58"),
                        new PecThread(9, 91, 166, "Electric Blue", "59"),
                        new PecThread(240, 249, 112, "Lemon Yellow", "60"),
                        new PecThread(227, 243, 91, "Fresh Green", "61"),
                        new PecThread(255, 153, 0, "Orange", "62"),
                        new PecThread(255, 240, 141, "Cream Yellow", "63"),
                        new PecThread(255, 200, 200, "Applique", "64")
            };
        }
    }
}
