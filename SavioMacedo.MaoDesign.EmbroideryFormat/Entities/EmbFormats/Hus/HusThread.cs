using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.EmbFormats.Hus
{
    public class HusThread : EmbThread
    {
        public HusThread(string hexColor, string description, string catalogNumb) : base(description: description, catalogNumber: catalogNumb, brand: "Hus", chart: "Hus")
        {
            SetColor(hexColor);
        }

        public static List<HusThread> GetHusThreads()
        {
            return new List<HusThread>()
            {
                new("000000", "Black", "026"),
                new("0000e7", "Blue", "005"),
                new("00c600", "Green", "002"),
                new("ff0000", "Red", "014"),
                new("840084", "Purple", "008"),
                new("ffff00", "Yellow", "020"),
                new("848484", "Grey", "024"),
                new("8484e7", "Light Blue", "006"),
                new("00ff84", "Light Green", "003"),
                new("ff7b31", "Orange", "017"),
                new("ff8ca5", "Pink", "011"),
                new("845200", "Brown", "028"),
                new("ffffff", "White", "022"),
                new("000084", "Dark Blue", "004"),
                new("008400", "Dark Green", "001"),
                new("7b0000", "Dark Red", "013"),
                new("ff6384", "Light Red", "015"),
                new("522952", "Dark Purple", "007"),
                new("ff00ff", "Light Purple", "009"),
                new("ffde00", "Dark Yellow", "019"),
                new("ffff9c", "Light Yellow", "021"),
                new("525252", "Dark Grey", "025"),
                new("d6d6d6", "Light Grey", "023"),
                new("ff5208", "Dark Orange", "016"),
                new("ff9c5a", "Light Orange", "018"),
                new("ff52b5", "Dark Pink", "010"),
                new("ffc6de", "Light Pink", "012"),
                new("523100", "Dark Brown", "027"),
                new("b5a584", "Light Brown", "029")
            };
        }
    }
}
