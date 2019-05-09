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
    public Point3D position = new Point3D();
    public Point3D velocity = new Point3D();
    public int generation = 5;
    public double minimumSpeed = 1;
    public double speedStep = 1;
    public int steps = 10;
    public double initialRadius = 1.0;

    public Point3D ObjectPosition { get; set; } = new Point3D();
    private Scene Scene { get; }
    private Mesh Triad { get; set; }
    private Mesh Arrow { get; set; }

    public MeteoriteInitializationForm(Scene scene)
    {
      Scene = scene;

      InitializeComponent();

      DistanceToBox.Items.Clear();
      foreach (IRenderable renderable in scene.RenderableObjects)
        if (renderable is Planet planet)
          DistanceToBox.Items.Add(renderable.Name);

      if (DistanceToBox.Items.Count>0)
        DistanceToBox.Text = DistanceToBox.Items[0].ToString();

      DistanceTrackBar.ValueChanged += ChangePosition;
      PositionRightAscensionTrackBar.ValueChanged += ChangePosition;
      PositionDeclinationTrackBar.ValueChanged += ChangePosition;

      SpeedTrackBar.ValueChanged += ChangeVelocity;
      VelocityRightAscensionTrackBar.ValueChanged += ChangeVelocity;
      VelocityDeclinationTrackBar.ValueChanged += ChangeVelocity;

      ShowTriad(); 
    }

    private void ShowTriad()
    {
      Triad = new Mesh();
      Triad.Name = "Meteorite Shower Position";
      TriadGeometry triad = new TriadGeometry();
      triad.Arrows.Transform.Rotation = EquatorialCoordinateSystem.Main.SystemRotation; 
      Triad.Children.Add(triad.Arrows);
      Arrow = TriadGeometry.GenerateArrow();
      Triad.Children.Add(Arrow);
      Scene.RenderableObjects.Add(Triad);
    }

    private void RemoveTriad()
    {
      Scene.RenderableObjects.Remove(Triad); 
    }


    private void RepositionTriad()
    {
      if (Triad == null)
        return;

      Triad.Transform.Position = position;

      Point3D arrowDirection = velocity.Normal;

      double pitch = -Math.Asin(arrowDirection.z);
      double yaw = Math.Atan2(arrowDirection.y, arrowDirection.x);
      double roll = 0; 

      Arrow.Transform.Rotation = new Quaternion(yaw,pitch,roll);
      
      Scene.Changed = true; 
    }

    /// <summary>
    /// Read the velocity from the trackbars and set the x, y and z boxes accordingly. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangeVelocity(object sender, EventArgs e)
    {
      double rightAscension = VelocityRightAscensionTrackBar.Value;
      double declination = VelocityDeclinationTrackBar.Value;

      Point3D direction = EquatorialCoordinateSystem.Main.EquatorialCoordinate(rightAscension, declination);
      velocity = direction * SpeedTrackBar.Value;

      VelocityXBox.Text = velocity.x.ToString();
      VelocityYBox.Text = velocity.y.ToString();
      VelocityZBox.Text = velocity.z.ToString();

      RepositionTriad(); 
    }

    /// <summary>
    /// Read the position from the trackbars and set the x, y and z boxes accordingly. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangePosition(object sender, EventArgs e)
    {
      double rightAscension = PositionRightAscensionTrackBar.Value;
      double declination = PositionDeclinationTrackBar.Value;

      Point3D direction = EquatorialCoordinateSystem.Main.EquatorialCoordinate(rightAscension, declination);
      position = direction * DistanceTrackBar.Value;

      position += ObjectPosition; 

      PositionXBox.Text = position.x.ToString();
      PositionYBox.Text = position.y.ToString();
      PositionZBox.Text = position.z.ToString();

      RepositionTriad(); 
    }

    /// <summary>
    /// Read the position from the x, y and z value, and set the trackbars accordingly.
    /// </summary>
    private void ReadPosition()
    {
      position = new Point3D(
        Convert.ToDouble(PositionXBox.Text),
        Convert.ToDouble(PositionYBox.Text),
        Convert.ToDouble(PositionZBox.Text));

      position -= ObjectPosition;
      double distance = position.Magnitude;
      double rightAscension = EquatorialCoordinateSystem.Main.DirectionRightAscension(position);  
      double declination = EquatorialCoordinateSystem.Main.DirectionDeclination(position);

      DistanceTrackBar.Value = distance;
      PositionRightAscensionTrackBar.Value = rightAscension;
      PositionDeclinationTrackBar.Value = declination;

      RepositionTriad(); 
    }

    /// <summary>
    /// Read the velocity from the x, y and z value, and set the trackbars accordingly. 
    /// </summary>
    private void ReadVelocity()
    {
      velocity = new Point3D(
        Convert.ToDouble(VelocityXBox.Text),
        Convert.ToDouble(VelocityYBox.Text),
        Convert.ToDouble(VelocityZBox.Text));

      double speed = velocity.Magnitude;
      double rightAscension = EquatorialCoordinateSystem.Main.DirectionRightAscension(velocity);
      double declination = EquatorialCoordinateSystem.Main.DirectionDeclination(velocity);

      SpeedTrackBar.Value = speed;
      VelocityRightAscensionTrackBar.Value = rightAscension;
      VelocityDeclinationTrackBar.Value = declination;

      RepositionTriad(); 
    }


    private void AddButton_Click(object sender, EventArgs e)
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

        AddMeteorShower();
        Close(); 
      }
      catch
      {
        MessageBox.Show("Invalid input.");
      }
    }

    private void AddMeteorShower()
    {
      MeteorShower meteorShower = new MeteorShower(position, velocity, generation, minimumSpeed, speedStep, steps, initialRadius);

      //give meteor shower a unique name. 
      int i = 0;
      bool ok = false;
      while (!ok)
      {
        ok = true;
        foreach (IRenderable renderable in Scene.RenderableObjects)
        {
          if (renderable.Name == meteorShower.Name)
          {
            i++;
            meteorShower.Name = "Meteor Shower " + i.ToString();
            ok = false;
            break;
          }
        }
      }

      Scene.RenderableObjects.Add(meteorShower);
    }

    private void DistanceToBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      string selected = DistanceToBox.Text; 
      foreach (IRenderable renderable in Scene.RenderableObjects)
        if (renderable is Planet planet)
          if (planet.Name == selected)
            ObjectPosition = planet.Position;

      ChangePosition(sender, e); 
    }
 
    private void PositionBox_Leave(object sender, EventArgs e)
    {
      ReadPosition();
    }

    private void PositionBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        ReadPosition(); 
    }

    private void VelocityBox_Leave(object sender, EventArgs e)
    {
      ReadVelocity(); 
    }

    private void VelocityBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        ReadVelocity(); 
    }

    private void MeteoriteInitializationForm_FormClosed(object sender, FormClosedEventArgs e)
    {
      RemoveTriad(); 
    }
  }
}
