using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.BitMapHelpers;
using SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic.Enums;
using SkiaSharp;

namespace SavioMacedo.MaoDesign.EmbroideryFormat.Entities.Basic
{
    //Here we have the basic class for the embroidery format, it contains the basic properties and methodes for the embroidery format
    public class EmbroideryBasic
    {
        private float _previousX;
        private float _previousY;

        //Here is a list of all the stitches in the embroidery
        public List<Stitch> Stitches { get; set; }
        //Here is a list of all the threads in the embroidery
        public List<EmbThread> Threads { get; set; }
        //Here is a dictionary of all the metadata in the embroidery
        public Dictionary<string, string> Metadata { get; set; }
        public FileFormat FileFormat { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public float ImageHeight { get; set; }
        public float ImageWidth { get; set; }
        public SKBitmap SkBitmap { get; set; }
        internal SKPoint translateStart;

        public EmbroideryBasic() => (_previousX, _previousY, Stitches, Threads, Metadata) = (0, 0, new List<Stitch>(), new List<EmbThread>(), new Dictionary<string, string>());

        public EmbroideryBasic(EmbroideryBasic p)
        {
            FileName = p.FileName;
            Metadata = new(p.Metadata);
            Threads = new(p.Threads);
            Stitches = new(p.Stitches);
        }

        public void Move(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.Jump, dX, dY);
        }

        public void MoveAbsolute(float x, float y)
        {
            AddStitchRelative(Command.Jump, x, y);
        }

        public void Stitch(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.Stitch, dX, dY);
        }

        public void StitchAbsolute(float x, float y)
        {
            AddStitchAbsolute(Command.Stitch, x, y);
        }

        public void Stop(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.Stop, dX, dY);
        }

        public void Trim(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.Trim, dX, dY);
        }

        public void ColorChange(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.ColorChange, dX, dY);
        }

        public void AddThread(EmbThread thread)
        {
            Threads.Add(thread);
        }

        public void AddThread(uint threadColor)
        {
            EmbThread thread = new EmbThread(color: threadColor);
            Threads.Add(thread);
        }

        public void AddThread(Tuple<string, dynamic> thread)
        {
            EmbThread threadObj = new EmbThread();

            switch (thread.Item1)
            {
                case "name":
                    threadObj.Description = (string)thread.Item2;
                    break;
                case "description":
                    threadObj.Description = (string)thread.Item2;
                    break;
                case "desc":
                    threadObj.Description = (string)thread.Item2;
                    break;
                case "brand":
                    threadObj.Brand = (string)thread.Item2;
                    break;
                case "manufacturer":
                    threadObj.Brand = (string)thread.Item2;
                    break;
                case "color":
                    {
                        if (thread.Item2 is int @int)
                        {
                            threadObj.Color = (uint)@int;
                        }
                        else if (thread.Item2 is string @string)
                        {
                            if (thread.Item2 == "random")
                            {
                                threadObj.Color = (uint)(0xFF000000 | new Random().Next(0, 0xFFFFFF));
                            }
                            if (@string.StartsWith("#"))
                            {
                                threadObj.SetColor(@string);
                            }
                        }
                        else if (thread.Item2 is List<int> @list)
                        {
                            threadObj.Color = (uint)((list[0] & 0xFF) << 16 | (list[1] & 0xFF) << 8 | list[2] & 0xFF);
                        }

                        break;
                    }
                case "hex":
                    threadObj.SetColor(thread.Item2);
                    break;
                case "id":
                    threadObj.CatalogNumber = thread.Item2;
                    break;
                case "catalog":
                    threadObj.CatalogNumber = thread.Item2;
                    break;
                default:
                    break;
            }

            Threads.Add(threadObj);
        }

        public void SequinEject(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.SequinEject, dX, dY);
        }

        public void SequinMode(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.SequinMode, dX, dY);
        }

        public void End(float dX = 0, float dY = 0)
        {
            AddStitchRelative(Command.End, dX, dY);
        }

        public void SetMetadata(string name, string value)
        {
            Metadata.Add(name, value);
        }

        public string GetMetadata(string name)
        {
            return Metadata.GetValueOrDefault(name, string.Empty);
        }

        public (float, float, float, float) Extents()
        {
            float minX = float.MaxValue;
            float min_y = float.MaxValue;
            float max_x = float.MinValue;
            float max_y = float.MinValue;

            foreach (Stitch stitch in Stitches)
            {
                if (stitch.X > max_x)
                {
                    max_x = stitch.X;
                }

                if (stitch.X < minX)
                {
                    minX = stitch.X;
                }

                if (stitch.Y > max_y)
                {
                    max_y = stitch.Y;
                }

                if (stitch.Y < min_y)
                {
                    min_y = stitch.Y;
                }
            }

            return (minX, min_y, max_x, max_y);
        }

        public int CountColorChanges()
        {
            return CountStitchCommands(Command.ColorChange);
        }

        public int CountStich()
        {
            return CountStitchCommands(Command.Stitch);
        }

        public int CountJump()
        {
            return CountStitchCommands(Command.Jump);
        }

        public int GetSegmentCount()
        {
            int count = 0;

            foreach (var stitch in Stitches)
            {
                switch (stitch.Command)
                {
                    case Command.Stitch:
                    case Command.Jump:
                        {
                            count++;
                            break;
                        }
                }
            }
            return count;
        }

        public IEnumerable<(Stitch[], EmbThread)> GetAsColorBlocks()
        {
            int threadIndex = 0;
            int lastPos = 0;
            Stitch[] stitchArray = Stitches.ToArray();

            for (var pos = 0; pos < stitchArray.Count(); pos++)
            {
                Stitch stitch = stitchArray[pos];
                if (stitch.Command != Command.ColorChange)
                    continue;

                EmbThread thread = GetThreadOrFiller(threadIndex);
                threadIndex++;
                yield return (stitchArray[lastPos..^pos], thread);
                lastPos = pos;
            }

            EmbThread threadc = GetThreadOrFiller(threadIndex);
            yield return (stitchArray[lastPos..], threadc);
        }

        public IEnumerable<Stitch[]> GetAsCommandBlocks()
        {
            int lastPost = 0;
            Command lastCommand = Command.NoCommand;
            Stitch[] stitches = Stitches.ToArray();

            for (var i = 0; i < stitches.Count(); i++)
            {
                Stitch stitch = stitches[i];
                Command command = stitch.Command;

                if (command == lastCommand || lastCommand == Command.NoCommand)
                {
                    lastCommand = command;
                    continue;
                }

                lastCommand = command;
                yield return stitches[lastPost..^i];
                lastPost = i;
            }
            yield return stitches[Range.StartAt(lastPost)];
        }



        public IEnumerable<(List<Stitch>, EmbThread, int)> GetAsSegmentsBlocks(EmbThread[] chart, int adjust_x = 0, int adjust_y = 0)
        {
            int color_index = 0;
            EmbThread current_thread = GetThreadOrFiller(color_index);
            color_index++;
            int stitched_x = 0;
            int stitched_y = 0;
            foreach (var command_block in GetAsCommandBlocks())
            {
                var block = new List<Stitch>();
                Command command = command_block[0].Command;
                if (command == Command.Jump)
                {
                    block.Add(new Stitch(stitched_x - adjust_x, stitched_y - adjust_y, command));
                    var last_pos = command_block[^1];
                    block.Add(new Stitch(last_pos.X - adjust_x, last_pos.Y - adjust_y, command));
                    int flag = 1;
                    yield return (block, current_thread, flag);
                }
                else if (command == Command.ColorChange)
                {
                    current_thread = GetThreadOrFiller(color_index);
                    color_index++;
                    int flag = 1;
                    continue;
                }
                else if (command == Command.Stitch)
                {
                    foreach (var stitch in command_block)
                    {
                        stitched_x = (int)stitch.X;
                        stitched_y = (int)stitch.Y;
                        block.Add(new Stitch(stitched_x - adjust_x, stitched_y - adjust_y, command));
                    }
                    int flag = 0;
                    yield return (block, current_thread, flag);
                }
                else
                {
                    continue;
                }
            }
        }


        public IEnumerable<(List<Stitch>, EmbThread)> GetAsStitchBlock()
        {
            List<Stitch> stichBlock = new List<Stitch>();
            EmbThread thread = GetThreadOrFiller(0);
            int threadIndex = 1;
            foreach (Stitch stitch in Stitches)
            {
                Command flag = stitch.Command;
                if (flag == Command.Stitch || flag == Command.Trim || flag == Command.Jump)
                {
                    stichBlock.Add(stitch);
                }
                else
                {
                    if (stichBlock.Count > 0)
                    {
                        yield return (stichBlock, thread);
                        stichBlock.Clear();
                    }

                    if (flag == Command.ColorChange)
                    {
                        thread = GetThreadOrFiller(threadIndex);
                        threadIndex++;
                    }
                }
            }

            if (stichBlock.Count > 0)
            {
                yield return (stichBlock, thread);
            }
        }

        public EmbThread GetThreadOrFiller(int index)
        {
            if (Threads.Count <= index)
            {
                return GetRandomThread();
            }

            return Threads[index];
        }

        public static EmbThread GetRandomThread()
        {
            EmbThread thread = new()
            {
                Color = (uint)(0xFF000000 | new Random().Next(0, 0xFFFFFF)),
                Description = "Random"
            };

            return thread;
        }
        private int CountStitchCommands(Command command)
        {
            return Stitches.Count(a => a.Command == command);
        }

        private void AddStitchRelative(Command command, float dX = 0, float dY = 0)
        {
            float x = _previousX + dX;
            float y = _previousY + dY;
            AddStitchAbsolute(command, x, y);
        }

        public void AddStitchAbsolute(Command command, float x = 0, float y = 0)
        {
            Stitches.Add(new Stitch(x, y, command));
            _previousX = x;
            _previousY = y;
        }

        public void UpdateSkBitMap(bool allowTransparency, bool hideMachinePath, float threadThickness, SKPointMode sKPointMode = SKPointMode.Polygon)
        {
            SKBitmap tempImage = ToBitmap(threadThickness, hideMachinePath, 10.0f, 254 / 400f, sKPointMode);
            SKBitmap result;

            int width = (int)(tempImage.Width * 1.0f);
            int height = (int)(tempImage.Height * 1.0f);

            if (width < 1 || height < 1)
                return;

            if (width != tempImage.Width || height != tempImage.Height)
            {
                SKRect destRect = new(0, 0, width, height);
                SKBitmap scaledImage = new(width, height);

                scaledImage.Resize(tempImage.Info, SKFilterQuality.High);

                using (var graphics = new SKCanvas(scaledImage))
                {
                    graphics.DrawBitmap(tempImage, destRect, null);
                }

                tempImage.Dispose();
                tempImage = scaledImage;
            }

            if (SkBitmap != null)
            {
                SkBitmap.Dispose();
                SkBitmap = null;
            }

            if (allowTransparency)
            {
                SkBitmap = new SKBitmap(tempImage.Width, tempImage.Height);
                using var graphics = new SKCanvas(SkBitmap);
                SKColor colorTransparency = SKColors.Transparent;
                using SKPaint tempPen = new()
                {
                    Style = SKPaintStyle.Fill,
                    Color = colorTransparency,
                    IsAntialias = true
                };

                int gridSize = 5;
                for (var xStart = 0; xStart * gridSize < SkBitmap.Width; xStart++)
                {
                    for (var yStart = 0; yStart * gridSize < SkBitmap.Height; yStart++)
                    {
                        if (xStart % 2 == yStart % 2)
                        {
                            graphics.DrawRect(xStart * gridSize, yStart * gridSize, gridSize, gridSize, tempPen);
                        }
                    }
                }

                graphics.DrawBitmap(tempImage, 0, 0);

                tempImage.Dispose();
            }
            else
            {
                SkBitmap = tempImage;
            }
        }

        public void FixColorCount()
        {
            int threadIndex = 0;
            bool isInitColor = true;

            foreach (var stitch in Stitches)
            {
                var data = stitch.Command;
                if (data == Command.Stitch || data == Command.ColorBreak)
                {
                    if (isInitColor)
                    {
                        threadIndex++;
                        isInitColor = false;
                    }
                }
                else if (data == Command.ColorChange || data == Command.ColorBreak)
                {
                    isInitColor = true;
                }
            }

            while (Threads.Count < threadIndex)
            {
                AddThread(GetThreadOrFiller(Threads.Count));
            }
        }

        public void InterpolateStopAsDuplicateColor(Command threadChangeCommand = Command.ColorChange)
        {
            int threadIndex = 0;
            for (var i = 0; i < Stitches.Count; i++)
            {
                Stitch stitch = Stitches[i];
                Command command = stitch.Command;
                if (command == Command.Stitch)
                    continue;
                else if (command == Command.ColorBreak || command == Command.ColorChange)
                    threadIndex++;
                else if (command == Command.Stop)
                {
                    try
                    {
                        Threads.Insert(threadIndex, Threads.ElementAt(threadIndex));
                        Stitches.ElementAt(i).Command = threadChangeCommand;
                        threadIndex++;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return;
                    }
                }
            }
        }

        public List<int> BuildUniquePalette(IEnumerable<EmbThread> threadPalette, List<EmbThread> threadList)
        {
            List<int> palette = new();

            foreach (var thread in threadList)
            {
                palette.Add(thread.FindNearestColorIndex(threadPalette));
            }

            return palette;
        }

        internal SKBitmap ToBitmap(float threadThickness, bool hideMachinePath, double filterUtglyStitchesThreshold, float scale, SKPointMode sKPointMode = SKPointMode.Polygon)
        {
            if (scale < 0.0000001f)
            {
                throw new ArgumentException("Scale must be > 0");
            }
            if (filterUtglyStitchesThreshold < 1.0)
            {
                throw new ArgumentException("Filter ungly stitches threshold must be at least 1.0");
            }
            if (threadThickness < 0.1)
            {
                throw new ArgumentException("Thread thickness must be at least 0.1");
            }

            int imageWidth = (int)((ImageWidth + threadThickness * 2) * scale);
            int imageHeight = (int)((ImageHeight + threadThickness * 2) * scale);
            float tempThreadThickness = threadThickness * scale;

            SKBitmap drawArea = new SKBitmap(imageWidth, imageHeight);

            if (Threads.Count == 0)
            {
                return drawArea;
            }

            using var xGraph = new SKCanvas(drawArea);
            int translateX = (int)(translateStart.X * scale);
            int translateY = (int)(translateStart.Y * scale);
            xGraph.Translate(tempThreadThickness + translateX, tempThreadThickness + translateY);

            EmbThread thread = Threads[0];
            using SKPaint tempPen = new SKPaint
            {
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                Color = thread.AsSkColor(),
                IsAntialias = true
            };

            List<OptimizedBlockData> optimizedBlocks = new();

            foreach (EmbThread thisBlock in Threads)
            {
                optimizedBlocks.AddRange(GetOptimizedDrawData(thisBlock, scale, hideMachinePath, filterUtglyStitchesThreshold));
            }

            foreach (OptimizedBlockData optBlock in optimizedBlocks)
            {
                tempPen.Color = optBlock.color;
                xGraph.DrawPoints(sKPointMode, optBlock.points, tempPen);
            }

            return drawArea;
        }

        private List<OptimizedBlockData> GetOptimizedDrawData(EmbThread block, float scale, bool hideMachinePath, double filterUglyStitchesThreshold)
        {
            List<OptimizedBlockData> retval = new();

            if (block.FancyLines.Count == 0)
            {
                return retval;
            }

            List<SKPoint> currentPoints = new();

            foreach (FancyLine thisStitch in block.FancyLines)
            {
                if (hideMachinePath && thisStitch.CalcLength() > filterUglyStitchesThreshold)
                {
                    if (currentPoints.Count != 0)
                    {
                        retval.Add(new OptimizedBlockData(block.AsSkColor(), currentPoints.ToArray()));
                    }
                    currentPoints = new List<SKPoint>();
                    continue;
                }

                if (currentPoints.Count == 0)
                {
                    currentPoints.Add(new SKPoint((int)(thisStitch.A.X * scale), (int)(thisStitch.A.Y * scale)));
                }
                currentPoints.Add(new SKPoint((int)(thisStitch.B.X * scale), (int)(thisStitch.B.Y * scale)));
            }

            if (currentPoints.Count != 0)
            {
                retval.Add(new OptimizedBlockData(block.AsSkColor(), currentPoints.ToArray()));
            }

            return retval;
        }

        public SKRect GetBound()
        {
            SKRect bounds;

            if (Stitches.Count == 0)
            {
                bounds = SKRect.Empty;
                return bounds;
            }
            Stitch stitch = Stitches[0];
            bounds = new SKRect(stitch.X, stitch.Y, stitch.X, stitch.Y);

            foreach (Stitch stitch1 in Stitches)
            {
                bounds.Inflate(stitch1.X, stitch1.Y);
            }

            return bounds;
        }

        internal void CreateStitchBlocks()
        {
            float prevX = 0;
            float prevY = 0;
            float maxX = 0;
            float minX = 0;
            float maxY = 0;
            float minY = 0;
            List<FancyLine> tempStitches = new();

            IEnumerable<(List<Stitch>, EmbThread)> stitchBlocks = GetAsStitchBlock();
            foreach (var stitchBlock in stitchBlocks)
            {
                EmbThread thread = Threads.FirstOrDefault(a => a == stitchBlock.Item2);
                foreach (var stitch in stitchBlock.Item1.ToList())
                {
                    FancyLine prevStitch = new(new SKPoint(prevX, prevY), new SKPoint(stitch.X, stitch.Y));
                    thread.FancyLines.Add(prevStitch);
                    prevY = stitch.Y;
                    prevX = stitch.X;
                }
            }

            (minX, minY, maxX, maxY) = Extents();
            ImageWidth = maxX - minX;
            ImageHeight = maxY - minY;
            translateStart.X = -minX;
            translateStart.Y = -minY;
        }

        internal void ToSkiaFormat()
        {
            float prevX = 0;
            float prevY = 0;
            float maxX = 0;
            float minX = 0;
            float maxY = 0;
            float minY = 0;
            List<FancyLine> tempStitches = new();

            IEnumerable<(List<Stitch>, EmbThread)> stitchBlocks = GetAsStitchBlock();
            foreach (var (stitches, embThread) in stitchBlocks)
            {
                EmbThread thread = Threads.FirstOrDefault(a => a == embThread);
                foreach (var stitch in stitches.ToList())
                {
                    FancyLine prevStitch = new FancyLine(new SKPoint(prevX, prevY), new SKPoint(stitch.X, stitch.Y));
                    thread.FancyLines.Add(prevStitch);
                    prevY = stitch.Y;
                    prevX = stitch.X;
                }
            }

            (minX, minY, maxX, maxY) = Extents();
            ImageWidth = maxX - minX;
            ImageHeight = maxY - minY;
            translateStart.X = -minX;
            translateStart.Y = -minY;
        }

        public void ConvertJumpsToTrim(int jumpsToRequireTrim = 3)
        {
            EmbroideryBasic tempPattern = (EmbroideryBasic)Activator.CreateInstance(GetType());
            int i = -1;
            int ie = Stitches.Count - 1;
            int count = 0;
            bool trimmed = true;
            while (i < ie)
            {
                i++;
                Stitch stitch = Stitches[i];
                Command command = stitch.Command;
                if (command == Command.Stitch || command == Command.SequinEject)
                    trimmed = true;
                else if (command == Command.ColorChange || command == Command.Trim)
                    trimmed = true;

                if (trimmed || command != Command.Jump)
                {
                    tempPattern.AddStitchAbsolute(command, stitch.X, stitch.Y);
                    continue;
                }

                while (i < ie && command == Command.Jump)
                {
                    i++;
                    stitch = Stitches[i];
                    command = stitch.Command;
                    count++;
                }

                if (command != Command.Jump)
                {
                    i--;
                }

                stitch = Stitches[i];
                if (count >= jumpsToRequireTrim)
                {
                    tempPattern.Trim();
                }

                count = 0;
                tempPattern.AddStitchAbsolute(stitch.Command, stitch.X, stitch.Y);
            }

            Stitches = tempPattern.Stitches;
        }

        public int Signed8(int b)
        {
            if (b > 127)
                return -256 + b;

            return b;
        }

        public int Signed16(int b)
        {
            b &= 0xFFFF;
            if (b > 0x7FFF)
                return -0x10000 + b;
            else
                return b;
        }

        public List<EmbThread> GetUniqueThreadList()
        {
            List<EmbThread> threads = new();
            foreach (EmbThread thread in Threads)
            {
                if (!threads.Contains(thread))
                {
                    threads.Add(thread);
                }
            }
            return threads;
        }

        public void FlipVertical()
        {
            for (var i = 0; i < Stitches.Count; i++)
            {
                Stitches[i].Y *= (float)-1.0;
            }
        }

        public void CorrectForMaxStitchLength(float maxStitchLength, float maxJumpLength)
        {
            if (Stitches.Count > 1)
            {
                int i, j, splits;
                float maxXY, maxLen, addX, addY;
                List<Stitch> newList = new();

                for (i = 1; i < Stitches.Count; i++)
                {
                    Stitch st = Stitches[i];
                    float xx = st.X;
                    float yy = st.Y;
                    float dx = Stitches[i - 1].X - xx;
                    float dy = Stitches[i - 1].Y - yy;

                    if (Math.Abs(dx) > maxStitchLength || Math.Abs(dy) > maxStitchLength)
                    {
                        maxXY = Math.Max(Math.Abs(dx), Math.Abs(dy));

                        if ((st.Command == (Command.Jump | Command.Trim)) != false)
                        {
                            maxLen = maxJumpLength;
                        }
                        else
                        {
                            maxLen = maxStitchLength;
                        }

                        splits = (int)Math.Ceiling(maxXY / maxLen);

                        if (splits > 1)
                        {
                            addX = dx / splits;
                            addY = dy / splits;

                            for (j = 1; j < splits; j++)
                            {
                                Stitch s = st;
                                s.X = xx + addX * j;
                                s.Y = yy + addY * j;
                                newList.Add(s);
                            }
                        }
                    }

                    newList.Add(st);
                }

                Stitches.Clear();
                Stitches = newList;
            }

            End();
        }
    }
}
