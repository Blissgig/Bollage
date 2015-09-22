using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BollageScreenSaver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToLower().Trim().Substring(0, 2) == "/s") //show
                {
                    try
                    {
                        //run the screen saver
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        ShowScreensaver();
                        Application.Run();
                    }
                    catch (Exception ex)
                    {
                    } 
                }
                else if (args[0].ToLower().Trim().Substring(0, 2) == "/p") //preview
                {
                    try
                    {
                        //show the screen saver preview
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm(new IntPtr(long.Parse(args[1])))); //args[1] is the handle to the preview window
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (args[0].ToLower().Trim().Substring(0, 2) == "/c") //configure
                {
                    try
                    {
                        //Show the options dialog so the user can add/remove folders
                        Options frmOptions = new Options();
                        frmOptions.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else //an argument was passed, but it wasn't /s, /p, or /c, so we don't care wtf it was
                {
                    try
                    {
                        //show the screen saver anyway
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        ShowScreensaver();
                        Application.Run();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else //no arguments were passed
            {
                try
                {
                    //run the screen saver
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    ShowScreensaver();
                    Application.Run();
                }
                catch (Exception ex)
                {
                }
            }
        }

        static void ShowScreensaver()
        {
            try
            {
                //Loop through all the computer's screens
                foreach (Screen screen in Screen.AllScreens)
                {
                    //creates a form just for that screen and passes it the bounds of that screen
                    MainForm frmScreenSaver = new MainForm(screen.Bounds);
                    frmScreenSaver.FormBorderStyle = FormBorderStyle.None;
                    frmScreenSaver.Show();
                    frmScreenSaver.Bounds = screen.Bounds; //There is an issue where the passed bounds does not always work.  
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
