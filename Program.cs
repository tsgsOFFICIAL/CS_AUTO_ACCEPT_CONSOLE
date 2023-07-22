using System;
using Tesseract;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace CS_AUTO_ACCEPT_CONSOLE
{
    class Program
    {
        #region Global variables
        public static bool runScanner = false;
        public static bool moveMouse = false;
        public static bool checkForCancel = false;
        public static double tempxstartpos = 0;
        public static double tempystartpos = 0;
        public static double _clickxpos = 0;
        public static double _clickypos = 0;
        public static string _title = "AutoAccept: ";
        public static ImageConverter _converter = new ImageConverter();
        #endregion
        #region Mouse stuff
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        #endregion
        static void Main()
        {
            #region debug
            //Thread.Sleep(5000);

            //Bitmap regionCancel = CaptureMyScreen(290, 48, 1440, 1010); //Capture the cancel button

            //regionCancel.Save(@"C:\Users\Marcus Jensen\Desktop\test data\og.png");
            //Sharpen(regionCancel).Save(@"C:\Users\Marcus Jensen\Desktop\test data\sharp_og.png");
            //AdjustContrast(regionCancel, 0).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_og_0.png");
            //AdjustContrast(regionCancel, 50).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_og_50.png");
            //AdjustContrast(regionCancel, 100).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_og_100.png");
            //AdjustContrast(Sharpen(regionCancel), 100).Save(@"C:\Users\Marcus Jensen\Desktop\test data\sharp_contrast_og_100.png");

            ////Convert to black & white
            //for (int i = 0; i < regionCancel.Width; i++)
            //{
            //    for (int j = 0; j < regionCancel.Height; j++)
            //    {
            //        Color colorValue = regionCancel.GetPixel(i, j); // Get the color pixel
            //        int averageValue = (colorValue.R + colorValue.B + colorValue.G) / 3; // get the average for black and white
            //        regionCancel.SetPixel(i, j, Color.FromArgb(averageValue, averageValue, averageValue)); // Set the value to new pixel
            //    }
            //}
            //regionCancel.Save(@"C:\Users\Marcus Jensen\Desktop\test data\bw.png");
            //Sharpen(regionCancel).Save(@"C:\Users\Marcus Jensen\Desktop\test data\sharp_bw.png");
            //AdjustContrast(regionCancel, 0).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_bw_0.png");
            //AdjustContrast(regionCancel, 50).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_bw_50.png");
            //AdjustContrast(regionCancel, 100).Save(@"C:\Users\Marcus Jensen\Desktop\test data\contrast_bw_100.png");
            //AdjustContrast(Sharpen(regionCancel), 100).Save(@"C:\Users\Marcus Jensen\Desktop\test data\sharp_contrast_bw_100.png");

            //regionCancel = AdjustContrast(Sharpen(regionCancel), 100);

            //TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            //Pix img = Pix.LoadFromMemory((byte[])_converter.ConvertTo(regionCancel, typeof(byte[])));
            //Page page = engine.Process(img);

            //string text = page.GetText();
            //float confidence = page.GetMeanConfidence();

            //System.IO.File.WriteAllText(@"C:\Users\Marcus Jensen\Desktop\test data\data.txt", $"\n\ntext: \"{text}\"\nConfidence: \"{confidence}%\"");

            //Environment.Exit(0);
            #endregion
            Thread scannerThread = new Thread(new ThreadStart(MainScanner));
            Thread menuThread = new Thread(new ThreadStart(ShowMenu));

            scannerThread.Start();
            menuThread.Start();
        }
        /// <summary>
        /// Sharpen an image
        /// </summary>
        /// <param name="image"></param>
        /// <returns>This method returns a sharpened image as a Bitmap</returns>
        public static Bitmap Sharpen(Bitmap image)
        {
            Bitmap sharpenImage = new Bitmap(image.Width, image.Height);

            int filterWidth = 3;
            int filterHeight = 3;
            int w = image.Width;
            int h = image.Height;

            double[,] filter = new double[filterWidth, filterHeight];

            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            double factor = 1.0;
            double bias = 0.0;

            Color[,] result = new Color[image.Width, image.Height];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    //=====[REMOVE LINES]========================================================
                    // Color must be read per filter entry, not per image pixel.
                    Color imageColor = image.GetPixel(x, y);
                    //===========================================================================

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + w) % w;
                            int imageY = (y - filterHeight / 2 + filterY + h) % h;

                            //=====[INSERT LINES]========================================================
                            // Get the color here - once per fiter entry and image pixel.
                            imageColor = image.GetPixel(imageX, imageY);
                            //===========================================================================

                            red += imageColor.R * filter[filterX, filterY];
                            green += imageColor.G * filter[filterX, filterY];
                            blue += imageColor.B * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    sharpenImage.SetPixel(i, j, result[i, j]);
                }
            }
            return sharpenImage;
        }
        /// <summary>
        /// Adjust the contrast of an image
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="Value"></param>
        /// <returns>This method returns a Bitmap that has been adjusted in the contrast</returns>
        public static Bitmap AdjustContrast(Bitmap Image, float Value)
        {
            Value = (100.0f + Value) / 100.0f;
            Value *= Value;
            Bitmap NewBitmap = (Bitmap)Image.Clone();
            BitmapData data = NewBitmap.LockBits(new Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height), ImageLockMode.ReadWrite, NewBitmap.PixelFormat);
            int Height = NewBitmap.Height;
            int Width = NewBitmap.Width;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;
                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            NewBitmap.UnlockBits(data);

            return NewBitmap;
        }
        /// <summary>
        /// Show the menu
        /// </summary>
        public static void ShowMenu()
        {
            while (true)
            {
                Console.Title = $"{_title} ({(runScanner ? "on" : "off")})";
                Console.Clear();
                Console.WriteLine($"Press '1' to toggle scanner");
                //Console.WriteLine($"Press '2' to toggle mouse movement ({(moveMouse ? "on" : "off")})");
                Console.WriteLine("Press '2' to quit");

                switch (Console.ReadKey(true).KeyChar)
                {
                    case '1':
                        runScanner = runScanner ? false : true; // Toggle from on & off
                                                                // Basically, if runScanner is true, set it to false, else set it to true
                        break;
                    //case '2':
                    //    moveMouse = moveMouse ? false : true; // Toggle from on & off
                    //                                          // Basically, if runScanner is true, set it to false, else set it to true
                    //    break;
                    case '2':
                        Environment.Exit(0);
                        break;
                }
            }
        }
        /// <summary>
        /// Main loop for running the scanner
        /// </summary>
        public static void MainScanner()
        {
            while (!runScanner)
            {
                Task.Delay(500).Wait();
                while (runScanner)
                {
                    if (moveMouse)
                    {
                        //Move the mouse
                        int move = 10;
                        Cursor.Position = new Point(Cursor.Position.X + move, Cursor.Position.Y + move);
                        Thread.Sleep(20);
                        Cursor.Position = new Point(Cursor.Position.X - move, Cursor.Position.Y - move);
                    }

                    Bitmap region = CaptureMyScreen(380, 30, 768, 460); //Capture the accept button

                    using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                    {
                        using (Pix img = Pix.LoadFromMemory((byte[])_converter.ConvertTo(region, typeof(byte[]))))
                        {
                            using (Page page = engine.Process(img))
                            {
                                string text = page.GetText();
                                float confidence = page.GetMeanConfidence();

                                //Console.WriteLine($"\"{text}\" / {confidence}");

                                if (text.ToLower().Contains("your match is ready!")/* && confidence > 0.90*/) //Pretty sure its right
                                {
                                    for (int i = 0; i < 10; i++)
                                    {
                                        Cursor.Position = new Point((int)_clickxpos + 10, (int)_clickypos + 10);
                                        uint X = (uint)Cursor.Position.X;
                                        uint Y = (uint)Cursor.Position.Y;
                                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0); //Left click 
                                    }

                                    checkForCancel = true;
                                    runScanner = false;

                                    Console.Title = $"{_title} (WAITING 30 SECONDS)";

                                    Task.Delay(30 * 1000).Wait();

                                    Console.Title = $"{_title} ({(runScanner ? "on" : "off")})";
                                }
                            }
                        }
                    }
                    Task.Delay(500).Wait();

                    if (checkForCancel)
                    {
                        Bitmap regionCancel = CaptureMyScreen(280, 45, 1440, 1010); //Capture the cancel button
                        Bitmap regionLobbySettings = CaptureMyScreen(800, 45, 120, 30); //Capture the cancel button

                        //Convert to black & white
                        for (int i = 0; i < regionCancel.Width; i++)
                        {
                            for (int j = 0; j < regionCancel.Height; j++)
                            {
                                Color colorValue = regionCancel.GetPixel(i, j); // Get the color pixel
                                int averageValue = (colorValue.R + colorValue.B + colorValue.G) / 3; // get the average for black and white
                                regionCancel.SetPixel(i, j, Color.FromArgb(averageValue, averageValue, averageValue)); // Set the value to new pixel
                            }
                        }

                        regionCancel = AdjustContrast(Sharpen(regionCancel), 100);

                        // Lobby Leader, start new match
                        using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                        {
                            using (Pix img = Pix.LoadFromMemory((byte[])_converter.ConvertTo(regionCancel, typeof(byte[]))))
                            {
                                using (Page page = engine.Process(img))
                                {
                                    string text = page.GetText();
                                    float confidence = page.GetMeanConfidence();

                                    if (text.ToLower().Contains("cancel search")/* && confidence > 0.90*/ || text.ToLower().Contains("go")) //Pretty sure its right
                                    {
                                        runScanner = true;

                                        // Press go again
                                        if (text.ToLower().Contains("go"))
                                        {
                                            for (int i = 0; i < 10; i++)
                                            {
                                                Cursor.Position = new Point((int)tempxstartpos + 10, (int)tempystartpos + 10);
                                                uint X = (uint)Cursor.Position.X;
                                                uint Y = (uint)Cursor.Position.Y;
                                                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0); //Left click
                                            }
                                        }
                                    }
                                }
                                //img.Save(@"C:\Users\Marcus Jensen\Desktop\png.png");
                            }
                        }

                        // Lobby settings / Ignore, keep running
                        using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                        {
                            using (Pix img = Pix.LoadFromMemory((byte[])_converter.ConvertTo(regionLobbySettings, typeof(byte[]))))
                            {
                                using (Page page = engine.Process(img))
                                {
                                    string text = page.GetText();
                                    float confidence = page.GetMeanConfidence();

                                    if (text.ToLower().Contains("lobby settings"))
                                    {
                                        runScanner = true;
                                    }
                                }
                            }
                        }
                        checkForCancel = false;
                    }
                    Console.Clear();
                    Console.WriteLine($"Press '1' to toggle scanner");
                    Console.WriteLine("Press '2' to quit");
                    Console.Title = $"{_title} ({(runScanner ? "on" : "off")})";
                }
            }
        }
        /// <summary>
        /// Take a screen capture assuming the screen is in 16:9 format
        /// </summary>
        /// <param name="xwidth">Width in pixels</param>
        /// <param name="xheight">Height in pixels</param>
        /// <param name="xstartpos">X Starting position in pixels</param>
        /// <param name="ystartpos">Y Starting position in pixels</param>
        /// <returns>This method returns a bitmap of the area</returns>
        private static Bitmap CaptureMyScreen(int xwidth, int xheight, int xstartpos = 0, int ystartpos = 0)
        {
            // Convert original to any format
            double temp = (double)xwidth / Screen.AllScreens[0].Bounds.Height * 100; // == 20,37037037037037 %
            double tempxwidth = Screen.AllScreens[0].Bounds.Height * temp / 100; // == 220 px

            temp = (double)xheight / Screen.AllScreens[0].Bounds.Width * 100; // == 4,166666666666667 %
            double tempxheight = Screen.AllScreens[0].Bounds.Width * temp / 100; // == 80 px

            temp = (double)xstartpos / Screen.AllScreens[0].Bounds.Width * 100; // == 44,27083333333333 %
            tempxstartpos = Screen.AllScreens[0].Bounds.Width * temp / 100; // == 850 px

            temp = (double)ystartpos / Screen.AllScreens[0].Bounds.Height * 100; // == 52,77777777777778 %
            tempystartpos = Screen.AllScreens[0].Bounds.Height * temp / 100; // == 570 px

            // Click position
            temp = (double)850 / Screen.AllScreens[0].Bounds.Width * 100; // == 44,27083333333333 %
            _clickxpos = Screen.AllScreens[0].Bounds.Width * temp / 100; // == 850 px

            temp = (double)570 / Screen.AllScreens[0].Bounds.Height * 100; // == 52,77777777777778 %
            _clickypos = Screen.AllScreens[0].Bounds.Height * temp / 100; // == 570 px


            try
            {
                //Creating a new Bitmap object
                Bitmap captureBitmap = new Bitmap((int)tempxwidth, (int)tempxheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                //Creating a Rectangle object which will capture our Current Screen
                Rectangle captureRectangle = Screen.AllScreens[0].Bounds;

                //Creating a New Graphics Object
                Graphics captureGraphics = Graphics.FromImage(captureBitmap);

                //Copying Image from The Screen
                if (xstartpos == 0 && ystartpos == 0)
                {
                    tempxstartpos = captureRectangle.Left;
                    tempystartpos = captureRectangle.Top;
                }

                captureGraphics.CopyFromScreen((int)tempxstartpos, (int)tempystartpos, 0, 0, captureRectangle.Size);

                //captureBitmap.Save($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\cstest\temp{tempcounter}.png", System.Drawing.Imaging.ImageFormat.Png);
                return captureBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}