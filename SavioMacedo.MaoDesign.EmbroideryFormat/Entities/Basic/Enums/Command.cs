namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums
{
    public enum Command
    {
        NoCommand,
        Stitch,
        Jump,
        Trim,
        Stop,
        End,
        ColorChange,
        SequinMode,
        SequinEject,
        ColorBreak = 0xE2
    }
}
