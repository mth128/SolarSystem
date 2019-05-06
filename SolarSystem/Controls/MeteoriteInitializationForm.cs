﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SolarSystem
{
  public partial class MeteoriteInitializationForm : Form
  {
    private Scene scene; 
    public Point3D position = new Point3D();
    public Point3D velocity = new Point3D();
    public int generation = 5;
    public double minimumSpeed = 1;
    public double speedStep = 1;
    public int steps = 10;
    public double initialRadius = 1.0;

    public MeteoriteInitializationForm(Scene scene)
    {
      InitializeComponent();

      DistanceToBox.Items.Clear();
      foreach (IRenderable renderable in scene.RenderableObjects)
        if (renderable is Planet planet)
          DistanceToBox.Items.Add(renderable.Name);

      if (DistanceToBox.Items.Count>0)
        DistanceToBox.Text = DistanceToBox.Items[0].ToString();
      
    }

    private void OKButton_Click(object sender, EventArgs e)
    {
      try
      {
        position = new Point3D(Convert.ToDouble(PositionXBox.Text),
          Convert.ToDouble(PositionYBox.Text),
          Convert.ToDouble(PositionZBox.Text) );
        velocity = new Point3D(Convert.ToDouble(VelocityXBox.Text),
          Convert.ToDouble(VelocityYBox.Text),
          Convert.ToDouble(VelocityZBox.Text));
        generation = Convert.ToInt32(DetailLevelTrackBar.Value);
        minimumSpeed = Convert.ToDouble(MinimumSpeedBox.Text);
        speedStep = Convert.ToDouble(SpeedStepBox.Text);
        steps = Convert.ToInt32(StepsBox.Text);
        initialRadius = Convert.ToDouble(InitialRadiusBox.Text); 
      }
      catch
      {
        MessageBox.Show("Invalid input.");
      }
    }
  }
}
