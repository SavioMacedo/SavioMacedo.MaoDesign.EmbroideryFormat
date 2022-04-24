using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public class Stitch
    {
        public Stitch(float x, float y, Command command)
        {
            X = x;
            Y = y;
            Command = command;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public Command Command { get; set; }
    }
}
