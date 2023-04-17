namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums
{
    public enum Command
    {
        NoCommand = -1,
        Stitch = 0,
        Jump = 1,
        Trim = 2,
        Stop = 4,
        End = 8,
        ColorChange = 16,
        Init = 32,
        TieOn = 64,
        TieOff = 128,
        SequinMode,
        SequinEject,
        ColorBreak = 0xE2
    }
}
