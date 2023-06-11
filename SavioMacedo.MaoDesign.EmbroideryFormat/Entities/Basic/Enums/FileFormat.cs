using System.ComponentModel;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums
{
    public enum FileFormat
    {
        [Description("PES - Brother/Bernina")]
        Pes,
        [Description("JEF - Janome")]
        Jef,
        [Description("HUS - Husqvarna")]
        Hus,
        [Description("DST - Tajima")]
        Dst,
        [Description("XXX - Compucon/Singer")]
        Xxx,
        [Description("PEC - Brother")]
        Pec,
        Bin
    }
}
