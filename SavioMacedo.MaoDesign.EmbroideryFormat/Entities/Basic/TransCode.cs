using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    public class TransCode
    {
        int jumps_before_trim = 3;

        public bool SplitLongJumps = true;
        public double MaxJumpLength = double.PositiveInfinity;
        bool splitLongStitches = true;
        public double MaxStitchLength = double.PositiveInfinity;

        public bool Snap = true;
        bool combineTrimmedJumps = true;
        public bool Tie_on = false;
        public bool Tie_off = false;
        bool fix_color_count = false;

        public bool require_jumps = true;
        private double initial_x = Double.NaN;
        private double initial_y = Double.NaN;

        int sequenceEnd;
        int currentCommand;
        int nextSequenceCommand;
        bool trimmed;
        double nextSequenceX;
        double nextSequenceY;
        int colorIndex;
        double lastx;
        double lasty;
        int lastCommand;

        public TransCode()
        {
            sequenceEnd = 0;
            currentCommand = (int)Command.NoCommand;
            lastx = Double.NaN;
            lasty = Double.NaN;
            nextSequenceCommand = (int)Command.NoCommand;
            nextSequenceX = Double.NaN;
            nextSequenceY = Double.NaN;
            colorIndex = 0;
            trimmed = true;
        }

        public void SetInitialPosition(double x, double y)
        {
            initial_x = x;
            initial_y = y;
        }

        public void transcode(EmbroideryBasic source)
        {
            EmbroideryBasic copy = new(source);
            source.Stitches.Clear();
            source.Threads.Clear();
            Transcode(copy, source);
        }

        private void Lookahead(List<Stitch> stitches, int index)
        {
            nextSequenceCommand = -1;
            Stitch stitch = stitches[index];
            Command currentCommand = stitch.Command;
            for (int j = index + 1, je = stitches.Count; j < je; j++)
            {
                Stitch stitchLookAhead = stitches[j];
                Command lookAheadCommand = stitchLookAhead.Command;
                if (currentCommand != lookAheadCommand)
                {
                    nextSequenceCommand = (int)lookAheadCommand;
                    Stitch stichLast = stitches[j - 1];
                    nextSequenceX = stichLast.X;
                    nextSequenceY = stichLast.Y;
                    sequenceEnd = j - index;
                    break;
                }
            } //look ahead.
        }

        private void TrimReady(List<Stitch> transcode, double x, double y)
        {
            if (trimmed) return;
            if ((Tie_off) && (currentCommand != (int)Command.TieOff))
            {
                transcode.Add(new Stitch((float)x, (float)y, Command.TieOff));
            }
            transcode.Add(new Stitch((float)x, (float)y, Command.Trim));
            trimmed = true;
        }

        private void StitchReady(List<Stitch> transcode, double x, double y)
        {
            if (!trimmed) return;
            if ((require_jumps) && (currentCommand == (int)Command.Trim))
            {
                JumpTo(transcode, x, y);
            }
            if ((trimmed && Tie_on) && (currentCommand != (int)Command.TieOn))
            {
                transcode.Add(new Stitch((float)x, (float)y, Command.TieOn));
            }
            trimmed = false;
        }

        private void JumpTo(List<Stitch> transcode, double x, double y)
        {
            if (SplitLongJumps)
            {
                StepToRange(transcode, x, y, MaxJumpLength, (int)Command.Jump);
            }
            transcode.Add(new Stitch((float)x, (float)y, Command.Jump));
        }

        private void StepToRange(List<Stitch> transcode, double x, double y, double length, int data)
        {
            double distanceX = lastx - x;
            double distanceY = lasty - y;
            if ((Math.Abs(distanceX) > length) || (Math.Abs(distanceY) > length))
            {
                double stepsX = Math.Abs(Math.Ceiling(distanceX / length));
                double stepsY = Math.Abs(Math.Ceiling(distanceY / length));
                double steps = Math.Max(stepsX, stepsY);
                double stepSizeX, stepSizeY;
                if (stepsX > stepsY)
                {
                    stepSizeX = distanceX / stepsX;
                    stepSizeY = distanceY / stepsX;
                }
                else
                {
                    stepSizeX = distanceX / stepsY;
                    stepSizeY = distanceY / stepsY;
                }
                for (double q = 0, qe = steps - 1, qx = lastx, qy = lasty; q < qe; q += 1, qx += stepSizeX, qy += stepSizeY)
                {
                    transcode.Add(new Stitch((float)Math.Round(qx), (float)Math.Round(qy), (Command)data));
                }
            }
        }

        public void Transcode(EmbroideryBasic from, EmbroideryBasic to)
        {
            List<Stitch> stitches = from.Stitches;
            List<Stitch> transcode = to.Stitches;
            int currentIndexEnd = stitches.Count();
            int currentIndex = 0;
            while (currentIndex < currentIndexEnd)
            {
                Stitch stitch = stitches[currentIndex];
                Command currentCommand = stitch.Command;
                double x = stitch.X;
                double y = stitch.Y;
                if (sequenceEnd <= 0)
                {
                    Lookahead(stitches, currentIndex);
                }
                if ((currentCommand == Command.NoCommand) && (currentCommand != Command.Init))
                {
                    if (!double.IsNaN(initial_y) && !double.IsNaN(initial_x))
                    {
                        transcode.Add(new Stitch((float)initial_x, (float)initial_y, Command.Init));
                        transcode.Add(new Stitch((float)x, (float)y, Command.Jump));
                    }
                    else
                    {
                        transcode.Add(new Stitch((float)x, (float)y, Command.Init));
                    }
                }
                switch (currentCommand)
                {
                    case Command.Init:
                        initial_x = x;
                        initial_y = y;
                        transcode.Add(new Stitch((float)x, (float)y, Command.Init));
                        break;
                    case Command.Trim:
                        TrimReady(transcode, x, y);
                        break;
                    case Command.Stitch:
                        StitchReady(transcode, x, y);
                        stitchTo(transcode, x, y);
                        break;
                    case JUMP:
                        if ((nextSequenceCommand == COLOR_CHANGE) ||
                                ((currentCommand == STITCH) && ((sequenceEnd - currentIndex) >= jumps_before_trim)))
                        {
                            TrimReady(transcode, x, y);
                        }
                        if (trimmed & combineTrimmedJumps)
                        {
                            x = nextSequenceX;
                            y = nextSequenceY;
                            currentIndex += sequenceEnd - 1;
                            sequenceEnd = 1;//currentIndex;
                        }
                        JumpTo(transcode, x, y);
                        break;
                    case COLOR_CHANGE:
                        TrimReady(transcode, x, y);
                        if (fix_color_count)
                        {
                            if (from.getThreadCount() >= colorIndex)
                            {
                                to.addThread(from.getThreadOrFiller(colorIndex));
                            }
                        }
                        transcode.add((float)x, (float)y, COLOR_CHANGE | (colorIndex++ << 8));
                        break;
                    case STOP:
                        TrimReady(transcode, x, y);
                        transcode.add((float)x, (float)y, STOP);
                        break;
                    case TIE_OFF:
                    case TIE_ON:

                    default:
                        transcode.add((float)x, (float)y, stitches.getData(currentIndex));
                        break;
                }
                lastCommand = currentCommand;
                lastx = x;
                lasty = y;
                sequenceEnd--;
                currentIndex++;
            }
        }

        public static TransCode GetTransCode()
        {
            return new();
        }
    }
}
