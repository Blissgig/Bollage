using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;


namespace BollageScreenSaver
{
    public partial class MainForm : Form
    {
        #region Comments
            //This code comes, in part, from an article on DreamInCode.net
            //http://www.dreamincode.net/forums/topic/74297-making-a-c%23-screen-saver/
            //The rest of this code was written for a Windows Desktop app called Bollage
            //available on Blissgig.com.  Bollage and Bollage Screen Saver are free for use
            //and you should not have been charged for these applications.
            //The source code I wrote is free for use in non-commercial applications.
            //Sincerely, James Rose July 2011.  New York, NY.  USA
        #endregion

        #region Preview API's
        [DllImport("user32.dll")]
            static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);
        #endregion

        #region Declares
            [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
            private static extern int BitBlt(
            IntPtr hdcDest,     // handle to destination DC (device context)
            int nXDest,         // x-coord of destination upper-left corner
            int nYDest,         // y-coord of destination upper-left corner
            int nWidth,         // width of destination rectangle
            int nHeight,        // height of destination rectangle
            IntPtr hdcSrc,      // handle to source DC
            int nXSrc,          // x-coordinate of source upper-left corner
            int nYSrc,          // y-coordinate of source upper-left corner
            System.Int32 dwRop  // raster operation code
            );

            [DllImport("user32.dll")]
            static extern IntPtr GetWindowDC(IntPtr hWnd);
        #endregion

        #region Private Members
            private ArrayList maryImages = new ArrayList();
            private Int16 miImageInterval = 500;    //Interval between images.  This is set at a random value, see ImageDisplay
            private Int16 miImageClearCount = 88;   //Number of images before the screen is cleared.  Set at random during load and after each clear
            private Int16 miImageCount = 0;
            private System.Threading.Thread mthrImage;
            private string msImageTypes = ".jpg,.jpeg,.png,.gif,.tiff,.bmp,";
            bool mbIsPreviewMode = false;
            private const int SRCCOPY = 0xcc0020;
        #endregion

        #region Public Properties
           
            public Int16 ImageInterval
            {
                get { return miImageInterval; }

                set
                {
                    if (value < 100)
                        value = 100;
                    //Just to insure that the loading doesn't go TOO fast
                    miImageInterval = value;
                }
            }

            public Int16 ImageClearCount
            {
                get { return miImageClearCount; }

                set { miImageClearCount = value; }
            }

            public Int16 ImageCount
            {
                get {return miImageCount; }

                set { miImageCount = value; }
            }

            public bool IsPreviewMode
            {
                get { return mbIsPreviewMode; }

                set { mbIsPreviewMode = value; }
            }
        #endregion

        #region Public Methods
            public MainForm()
            {
                InitializeComponent();
            }

            //This constructor is passed the bounds this form is to show in
            //It is used when in normal mode
            public MainForm(Rectangle Bounds)
            {
                InitializeComponent();
                this.Bounds = Bounds;
                //hide the cursor
                Cursor.Hide();
            }

            //This constructor is the handle to the select screensaver dialog preview window
            //It is used when in preview mode (/p)
            public MainForm(IntPtr PreviewHandle)
            {
                InitializeComponent();

                //set the preview window as the parent of this window
                SetParent(this.Handle, PreviewHandle);

                //make this a child window, so when the select screensaver dialog closes, this will also close
                SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

                //set our window's size to the size of our window's new parent
                Rectangle ParentRect;
                GetClientRect(PreviewHandle, out ParentRect);
                this.Size = ParentRect.Size;

                //set our location at (0, 0)
                this.Location = new Point(0, 0);

                IsPreviewMode = true;
            }
        #endregion

        #region Private Methods
            private void MainForm_Shown(object sender, EventArgs e)
            {
                try
                {
                    string sFileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Application.ProductName + "\\BollageFolderList.txt";
                    Random objRandom = new Random();
                    ImageClearCount = Convert.ToInt16(objRandom.Next(88, 488));
                    objRandom = null;

                    //It is possible that the user has not added any folders
                    if (File.Exists(sFileName))
                    {
                        StreamReader rdrFolders = File.OpenText(@sFileName);

                        while (!rdrFolders.EndOfStream)
                        {
                            string sLine = rdrFolders.ReadLine();

                            if (Directory.Exists(sLine))
                            {
                                System.IO.DirectoryInfo dirFolder = new DirectoryInfo(sLine);
                                WorkWithDirectory(dirFolder);
                                dirFolder = null;
                            }
                        }
                        rdrFolders.Close();
                        rdrFolders = null;
                        if (maryImages.Count > 0)
                        {
                            ImageDisplay();
                        }
                        else
                        {
                            NoImages(); //No images in any of the folders
                        }
                    }
                    else
                    {
                        NoImages(); //No Folder(s) added.
                    }
                }
                catch (Exception ex)
                {
                }
            }

            private void WorkWithDirectory(DirectoryInfo aDir)
            {
	        try {
		        WorkWithFiles(aDir);

                System.IO.DirectoryInfo[] subDirs = aDir.GetDirectories();
                
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    WorkWithDirectory(dirInfo);
                }

                subDirs = null;

	        } catch (Exception ex) 
            {
	        }
        }

            private void WorkWithFiles(DirectoryInfo aDir)
            {
	        try {
                System.IO.FileInfo[] files = aDir.GetFiles("*.*"); 
                
                foreach (System.IO.FileInfo filFile in files)
                {
                    if (msImageTypes.IndexOf(filFile.Extension.ToLower()) > -1)
                    {
                        maryImages.Add(filFile.FullName);
                    }
                }

	        } catch (Exception ex) 
            {   
	        }
        }

            private void ImageDisplay()
            {
            try
            {
                Random objRandom = new Random();
                Int16 iValue = Convert.ToInt16(objRandom.Next(0, (maryImages.Count - 1)));
                String sFilePath = Convert.ToString(maryImages[iValue]);
                
                //Clear the images once every random interval
                //This prevents the background from becoming too cluttered 
                //and/or one image, or partial image staying on the screen for too long
                miImageCount += 1;

                if (this.ImageCount > this.ImageClearCount)
                {
                    //Reset, obviously
                    this.ImageCount = 0;
                    //Get a new random number of the number of images displayed before the screen is cleared
                    //88 and 488 because my wife's favorite number is 8.  
                    this.ImageClearCount = Convert.ToInt16(objRandom.Next(88, 488));

                    Graphics g = this.CreateGraphics();
                    this.BackColor = Color.Black;
                    g.Clear(Color.Black);
                    g.Dispose();
                    g = null;
                }
                
                if (File.Exists(sFilePath) == true)
                {
                    //Randomize the image's border width and color, though basically white.
                    Image imgPhoto = Image.FromFile(sFilePath);
                    imgPhoto = ImageResize(imgPhoto);
                    Int16 iBorderWidth = Convert.ToInt16(objRandom.Next(2, 6));
                    Bitmap bmpBorder = new Bitmap((imgPhoto.Width + (iBorderWidth * 2)), (imgPhoto.Height + (iBorderWidth * 2)));
                    Graphics gr = Graphics.FromImage(bmpBorder);
                    Int16 iRed = Convert.ToInt16(objRandom.Next(200, 255));
                    Int16 iGreen = Convert.ToInt16(objRandom.Next(200, 255));
                    Int16 iBlue = Convert.ToInt16(objRandom.Next(200, 255));
                    SolidBrush brsBorder = new SolidBrush(Color.FromArgb(255, iRed, iGreen, iBlue));
                    Int16 iHighValue = Convert.ToInt16((this.Width - bmpBorder.Width));
                    Int16 iX = Convert.ToInt16(objRandom.Next(0, (iHighValue < 0 ? 0 : iHighValue)));
                    iHighValue = Convert.ToInt16(this.Height - bmpBorder.Height);
                    Int16 iY = Convert.ToInt16(objRandom.Next(0, (iHighValue < 0 ? 0 : iHighValue)));
                    Rectangle recPicture = new Rectangle(iX, iY, bmpBorder.Width, bmpBorder.Height);
                    Point pntCenter = new Point(iX, iY);
                    double iRotate = objRandom.Next(-20, 20);  //+ or - 20 degrees from level
                    Image imgOutput = default(Image);
                    float iLoop;


                    gr.FillRectangle(brsBorder, 0, 0, imgPhoto.Width, bmpBorder.Height);
                    gr.DrawImage(imgPhoto, iBorderWidth, iBorderWidth, (imgPhoto.Width - (iBorderWidth * 2)), (bmpBorder.Height - (iBorderWidth * 2)));

                    bmpBorder = ImageRotate(bmpBorder, iRotate);

                    //Fade in.
                    gr = this.CreateGraphics();

                    for (iLoop = 0; iLoop <= 1; iLoop += .1f)
                    {
                        imgOutput = ImageOpacity(bmpBorder, iLoop);
                        gr.DrawImage(imgOutput, pntCenter);
                        System.Threading.Thread.Sleep(88);
                    }


                    //Reset the thread interval to a random value between 1/4 of a second and 4 seconds.
                    //This is so that the display of images is slightly random.
                    this.ImageInterval = Convert.ToInt16(objRandom.Next(250, 4000));

                    if ((mthrImage != null))
                    {
                        mthrImage = null;
                    }

                    //Thread the Pause so the application can still be moved or resized. 
                    //SCREEN SAVER NOTE: This is not really nessasary for the screen saver,
                    //but since this code came from the desktop app, and it won't hurt to have 
                    //it here, I'm leaving it.  So there.
                    mthrImage = new System.Threading.Thread(this.ImagePause);
                    mthrImage.IsBackground = true;
                    mthrImage.Start();


                    //Cleanup
                    gr.Dispose();
                    gr = null;
                    imgPhoto.Dispose();
                    imgPhoto = null;
                    brsBorder.Dispose();
                    brsBorder = null;
                    bmpBorder.Dispose();
                    bmpBorder = null;
                    imgOutput.Dispose();
                    imgOutput = null;
                }

                objRandom = null;
            }

            catch (Exception ex)
            {
                ImageDisplay(); //If there is an error, get another image
            }
            finally
            {
                GC.Collect();
            }
        }

            private void ImagePause()
            {
                try
                {
                    System.Threading.Thread.Sleep(this.ImageInterval);

                    ImageDisplay();

                }
                catch (Exception ex)
                {
                }
            }

            private void NoImages()
            {
                try
                {
                    SizeF StringSize = new SizeF();
                    string sValue = "No images available or no folders selected.";
                    Graphics gr = this.CreateGraphics();
                    Int16 iLeft;
                    Font fntNoImages = new Font("Microsoft Sans Serif", 20, FontStyle.Bold);

                    StringSize = gr.MeasureString(sValue, fntNoImages);
                    iLeft = Convert.ToInt16((this.Width / 2) - (StringSize.Width / 2));
                    gr.DrawString(sValue, fntNoImages, new SolidBrush(Color.White), iLeft, ((this.Height / 2) - 40));
                    gr.Dispose();
                    gr = null;
                }
                catch (Exception ex)
                {
                }
            }

            private Bitmap ImageRotate(Bitmap bmpInput, double dAngle)
            {
                Bitmap bmpReturn = bmpInput;

                try
                {
                    float iWidth = bmpInput.Width;
                    float iHeight = bmpInput.Height;
                    Point[] pntCorners = { new Point(0, 0), new Point(Convert.ToInt16(iWidth), 0), new Point(0, Convert.ToInt16(iHeight)), new Point(Convert.ToInt16(iWidth), Convert.ToInt16(iHeight)) };

                
                    float cx = iWidth / 2;
                    float cy = iHeight / 2;
                    long i = 0;


                    for (i = 0; i <= 3; i++)
                    {
                        pntCorners[i].X -= Convert.ToInt16(cx);
                        pntCorners[i].Y -= Convert.ToInt16(cy);
                    }

                
                    //float theta = float.Parse(dAngle) * PI / 180.0;
                    double theta = dAngle * Math.PI / 180.0;
                    double sin_theta = Math.Sin(Convert.ToDouble(theta));
                    double cos_theta = Math.Cos(Convert.ToDouble(theta));
                    float X = 0;
                    float Y = 0;

                    for (i = 0; i <= 3; i++)
                    {
                        X = pntCorners[i].X;
                        Y = pntCorners[i].Y;

                        pntCorners[i].X = Convert.ToInt16(X * cos_theta + Y * sin_theta);
                        pntCorners[i].Y = Convert.ToInt16(-X * sin_theta + Y * cos_theta);
                    }

                    float xmin = pntCorners[0].X;
                    float ymin = pntCorners[0].Y;

                    for (i = 1; i <= 3; i++)
                    {
                        if (xmin > pntCorners[i].X)
                            xmin = pntCorners[i].X;
                        if (ymin > pntCorners[i].Y)
                            ymin = pntCorners[i].Y;
                    }

                    for (i = 0; i <= 3; i++)
                    {
                        pntCorners[i].X -= Convert.ToInt16(xmin);
                        pntCorners[i].Y -= Convert.ToInt16(ymin);
                    }

                    Bitmap bmpOutput = new Bitmap(Convert.ToInt32(-2 * xmin), Convert.ToInt32(-2 * ymin));

                    Graphics gr = Graphics.FromImage(bmpOutput);

                    Array.Resize(ref pntCorners, 3);

                    gr.DrawImage(bmpInput, pntCorners);


                    bmpReturn = bmpOutput;

                    //Cleanup
                    bmpOutput.Dispose();
                    bmpOutput = null;
                    gr.Dispose();
                    gr = null;
                }
                catch (Exception ex)
                {
                }
            
            return bmpReturn;

        }

            private Image ImageResize(Image imgOriginal)
            {
            Image imgReturn = imgOriginal;

            try
            {
                Random objRandom = new Random();
                double dResize = objRandom.NextDouble();
                if (dResize < 0.3)
                    dResize = 0.4;
                Int16 iWidth = Convert.ToInt16(imgOriginal.Width * dResize);
                Int16 iHeight = Convert.ToInt16(imgOriginal.Height * dResize);

                //'To insure that images as smaller than the display
                if (iWidth > this.Width)
                {
                    dResize = objRandom.Next(3, 5);
                    iWidth = Convert.ToInt16(this.Width / dResize);
                    iHeight = Convert.ToInt16(this.Height / dResize);
                }

                if (iHeight > this.Height)
                {
                    dResize = objRandom.Next(3, 5);
                    iWidth = Convert.ToInt16(this.Width / dResize);
                    iHeight = Convert.ToInt16(this.Height / dResize);
                }
                Bitmap imgResize = new Bitmap(iWidth, iHeight);
                Graphics g = Graphics.FromImage(imgResize);


                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(imgOriginal, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(0, 0, imgOriginal.Width, imgOriginal.Height), GraphicsUnit.Pixel);

                g.Dispose();

                imgOriginal.Dispose();

                imgReturn = imgResize;


            }
            catch (Exception ex)
            {
               
            }
            
            return imgReturn;

        }

            private static Image ImageOpacity(Image imgPic, float fOpacity)
            {
            Bitmap bmpReturn = new Bitmap(imgPic.Width, imgPic.Height);

            try
            {
                Graphics gfxPic = Graphics.FromImage(bmpReturn);
                ColorMatrix cmxPic = new ColorMatrix();
                ImageAttributes iaPic = new ImageAttributes();


                cmxPic.Matrix33 = fOpacity;
                iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gfxPic.DrawImage(imgPic, new Rectangle(0, 0, bmpReturn.Width, bmpReturn.Height), 0, 0, imgPic.Width, imgPic.Height, GraphicsUnit.Pixel, iaPic);

                gfxPic.Dispose();


            }
            catch (Exception ex)
            {
            }
            
            return bmpReturn;
        }
        #endregion

        #region User Input
            private void MainForm_KeyDown(object sender, KeyEventArgs e)
            {
                if (!IsPreviewMode) //disable exit functions for preview
                {
                    Application.Exit();
                }
            }

            private void MainForm_Click(object sender, EventArgs e)
            {
                if (!IsPreviewMode) //disable exit functions for preview
                {
                    Application.Exit();
                }
            }

            //start off OriginalLoction with an X and Y of int.MaxValue, because
            //it is impossible for the cursor to be at that position. That way, we
            //know if this variable has been set yet.
            Point OriginalLocation = new Point(int.MaxValue, int.MaxValue);

            private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsPreviewMode) //disable exit functions for preview
            {
                //see if originallocat5ion has been set
                if (OriginalLocation.X == int.MaxValue & OriginalLocation.Y == int.MaxValue)
                {
                    OriginalLocation = e.Location;
                }
                //see if the mouse has moved more than 20 pixels in any direction. If it has, close the application.
                if (Math.Abs(e.X - OriginalLocation.X) > 20 | Math.Abs(e.Y - OriginalLocation.Y) > 20)
                {
                    Application.Exit();
                }
            }
        }

        #endregion
    }
}
