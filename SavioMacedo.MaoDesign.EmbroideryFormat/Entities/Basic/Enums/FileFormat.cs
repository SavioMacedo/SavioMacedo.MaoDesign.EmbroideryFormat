using System.ComponentModel;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums
{
    public enum FileFormat
    {
        [Description("application/pes")]
        Pes,
        [Description("application/jef")]
        Jef,
        [Description("application/hus")]
        Hus,
        [Description("application/dst")]
        Dst,
        [Description("application/xxx")]
        Xxx,
        [Description("application/pec")]
        Pec,
        Bin
    }
}
