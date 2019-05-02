﻿//Copyright Maarten 't Hart 2019
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SolarSystem
{
  static class Program
  {
    private static bool running = true; 
    public static bool Running()
    {
      return running;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
      running = false; 
    }
  }
}
