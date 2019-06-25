﻿//Copyright Maarten 't Hart 2019
using OpenGL;
using System;
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
  public partial class MainForm : Form
  {
    private double timeStep = 0; 
    private bool SimulationRunning { get; set; } = false;
    private bool SimulationWorkerReady { get; set; } = true;
    private HistoricDateTime PreviousDateTime { get; set; } = new HistoricDateTime(); 
    private HistoricDateTime DateTime { get; set; } = new HistoricDateTime(); 
    private bool TimeUnlocked { get; set; } = true; 
    private CelestialPropertiesForm celestialPropertiesForm = new CelestialPropertiesForm();
    private MeteoriteInitializationForm meteoriteInitializationForm;
    private ColorMapForm ColorMapForm { get; } = new ColorMapForm();

    private IRenderable ActiveObject { get; set; }

    private BackgroundWorker SimulationWorker { get; set; } = new BackgroundWorker();
    private double TimeStep
    {
      get => timeStep;
      set
      {
        timeStep = value; 
        CoreDll.SetTimeStep(value);
      }
    } 

    private CelestialPropertiesForm CelestialPropertiesForm
    {
      get
      {
        if (celestialPropertiesForm.IsDisposed)
        {
          celestialPropertiesForm = new CelestialPropertiesForm();
          SetSelectedObject(); 
        }
        return celestialPropertiesForm;
      }
    }

    private double Exxageration
    {
      get => Scene.Exxageration;
      set => Scene.Exxageration = value; 
    }
    private Camera Camera => GlView.Camera;
    private Scene Scene => GlView.Scene;

    public Planet Sun { get; private set; }
    public Planet Mercury { get; private set; }
    public Planet Venus { get; private set; }
    public Planet Earth { get; private set; }
    public Planet Moon { get; private set; }
    public Planet Mars { get; private set; }
    public Planet Jupiter { get; private set; }
    public Planet Saturn { get; private set; }
    public Planet Uranus { get; private set; }
    public Planet Neptune { get; private set; }
    public Planet Pluto { get; private set; }

    //Jupiters moons
    public Planet Io { get; private set; }
    public Planet Europa { get; private set; }
    public Planet Ganymede { get; private set; }
    public Planet Callisto { get; private set; }

    //Saturns moons
    public Planet Titan { get; private set; }
    public Planet Mimas { get; private set; }
    public Planet Enceladus { get; private set; }
    public Planet Tethys { get; private set; }
    public Planet Dione { get; private set; }
    public Planet Rhea { get; private set; }
    public Planet Hyperion { get; private set; }
    public Planet Iapetus { get; private set; }

    //Uranus moons
    public Planet Miranda { get; private set; }
    public Planet Ariel { get; private set; }
    public Planet Umbriel { get; private set; }
    public Planet Titania { get; private set; }
    public Planet Oberon { get; private set; }

    //Neptunes moon
    public Planet Triton { get; private set; }

    //Plutos moons
    public Planet Charon { get; private set; }



    public MainForm()
    {
      SplashScreen splashScreen = new SplashScreen();
      splashScreen.Show(); 
      InitializeComponent();

      //InitializeEarth(null, null); 
      InitializePlanets(); 
      TimeStep = 1.0 / 86400;
      Scene.SetAsMainScene();
      SetDateTime(new HistoricDateTime(0.0));
      GlView.Lookat(Earth, Earth.Name);
      splashScreen.Close();
      splashScreen.Dispose(); 
    }

    private void InitializeEarth(object sender, DoWorkEventArgs e)
    {
      Earth = AddPlanet(SolarSystemPlanet.Earth);
      Earth.SetColorMap(new ColorMap("Earth")); 
    }
       
    private void TestButton_Click(object sender, EventArgs e)
    { 
      Camera.Eye = new PositionObject(-10, 0, 0);
    }

    private void Add(IRenderable renderable)
    {
      Scene.RenderableObjects.Add(renderable); 
    }

    private Planet AddPlanet(SolarSystemPlanet planetID)
    {
      foreach (IRenderable renderable in Scene.RenderableObjects)
        if (renderable is Planet existingPlanet)
          if (existingPlanet.PlanetID == planetID)
            return existingPlanet;

      Planet planet = new Planet(planetID);
      planet.RenderableObject.RenderGeometry.SetGeodesicGrid(9);
      Add(planet);
      GlView.Lookat(planet, planet.Name);
      return planet;
    }

    private Planet AddSun()
    {
      if (Sun != null)
        return Sun; 

      Sun = AddPlanet(SolarSystemPlanet.Sun);
      Sun.RenderableObject.UseLight = false;
      Scene.SunLight = new SunLight(Sun);
      Scene.Lights.Add(Scene.SunLight);
      Camera.Light.On = false;
      return Sun; 
    }

    private void ForceRender()
    {
      GlView.Refresh(); 
    }

    /*private void TetrahedronTestButton_Click(object sender, EventArgs e)
    {
      Mesh mesh = new Mesh
      {
        vertices = new double[]
        {
          -1, 0.5, 0,
          1, 0.5, 0,
          0, -1, 0,
          0, 0, 1 },

        indices = new int[]
        {
          2, 1, 0,
          3, 0, 1,
          3, 1, 2,
          3, 2, 0
        },

        colors = new float[]
        { 0, 1, 0, 1,
          1, 0, 0, 1,
          0, 0, 1, 1,
          1, 1, 0, 1}
      };

      Scene.RenderableObjects.Add(mesh); 
    }*/

    /*private void SampleFormTestButton_Click(object sender, EventArgs e)
    {
      using (HelloTriangle.ANGLE.SampleForm form = new HelloTriangle.ANGLE.SampleForm())
        form.ShowDialog(); 
    }*/

    private void UpdateTimer_Tick(object sender, EventArgs e)
    {
      DateTime.TotalDays = CoreDll.GetTime(); 
      if (PreviousDateTime.TotalDays != DateTime.TotalDays)
      {
        DisplayDateTime();
        PreviousDateTime.TotalDays = DateTime.TotalDays;
        UpdatePlanetPositions(); 
      }

      if (Scene.Changed || Camera.Changed)
        ForceRender();

      if (SceneContentBox.Items.Count != Scene.RenderableObjects.Count)
      {
        SceneContentBox.Items.Clear();
        foreach (IRenderable renderable in Scene.RenderableObjects)
          SceneContentBox.Items.Add(renderable.Name, renderable.On);
      }
    }

    private void UpdatePlanetPositions()
    {
      foreach (IRenderable renderable in Scene.RenderableObjects)
        if (renderable is Planet planet)
          planet.TimeUpdate();

      Scene.Changed = true; 
    }

    private void ExxagerateTestButton_Click(object sender, EventArgs e)
    {
      double Exxageration = 25;
      foreach (IRenderable renderable in Scene.RenderableObjects)
        if (renderable is Planet planet)
          planet.SetExxageration(Exxageration);
      
    }

    private void ExxagerationBar_Scroll(object sender, EventArgs e)
    {
      double exxageration = ExxagerationBar.Value;
      exxageration /= 1000;
      exxageration *= exxageration;
      Math.Round(exxageration, 2); 
      ExxagerationTextBox.Text = exxageration.ToString();

      Exxageration = exxageration;  
    }

    private void ExxagerationTextBox_Apply(object sender, EventArgs e)
    {
      try
      {
        double exxageration = Convert.ToDouble(ExxagerationTextBox.Text);
        if (exxageration == Exxageration)
          return;
        if (exxageration < 0)
          exxageration = 0;
        else if (exxageration > 100)
          ExxagerationBar.Value = 10000;
        else
          ExxagerationBar.Value = Convert.ToInt32(exxageration * exxageration);

        Exxageration = exxageration; 
      }
      catch
      {
        ExxagerationTextBox.Text = Exxageration.ToString(); 
      }
    }

    private void ExxagerationTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        ExxagerationTextBox_Apply(sender, e); 
    }

    private void SceneContentBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      bool on = (e.NewValue == CheckState.Checked);
      if (e.Index >= Scene.RenderableObjects.Count)
        return;
      Scene.RenderableObjects[e.Index].On = on;      
    }

    private void ColorMapEditorButton_Click(object sender, EventArgs e)
    {
      try
      {
        if (ColorMapForm.ShowDialog() != DialogResult.OK)
          return;
        if (SceneContentBox.SelectedIndex >= 0 && SceneContentBox.SelectedIndex < Scene.RenderableObjects.Count)
          Scene.RenderableObjects[SceneContentBox.SelectedIndex].SetColorMap(ColorMapForm.ColorMap);
      }
      catch
      {
        MessageBox.Show("Cannot apply color.");
      }
    }

    private void TestImageButton_Click(object sender, EventArgs e)
    {
      Mesh texture = new Mesh()
      {
        Name = "texture test"
      };
      texture.InitializeAsTexture(@"Resource\Images\wall.jpg");
      Scene.RenderableObjects.Add(texture); 
    }

    private void PropertiesBoxButton_Click(object sender, EventArgs e)
    {
      CelestialPropertiesForm.Owner = this; 
      CelestialPropertiesForm.Show(); 
    }

    private void SceneContentBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      SetSelectedObject(); 
    }

    private void SetSelectedObject()
    {
      try
      {
        if (SceneContentBox.SelectedIndex >= Scene.RenderableObjects.Count || SceneContentBox.SelectedIndex < 0)
        {
          CelestialPropertiesForm.ActiveObject = ActiveObject = null; 
          return;
        }
        CelestialPropertiesForm.ActiveObject = ActiveObject = Scene.RenderableObjects[SceneContentBox.SelectedIndex];
      }
      catch
      {

      }
    }

    private void TestDateButton_Click(object sender, EventArgs e)
    {
      using (DateTesterForm form = new DateTesterForm())
        form.ShowDialog(); 
    }

    private void SetMonth(string monthName)
    {
      MonthDropDown.Text = monthName;
      SendDate(); 
    }

    private void SetADBC(string value)
    {
      ADBCDropDown.Text = value;
      SendDate(); 
    }

    private void MonthStripMenuItem_Click(object sender, EventArgs e)
    {
      SetMonth((sender as ToolStripMenuItem).Text);
    }

    private void DateTimeBox_Leave(object sender, EventArgs e)
    {
      SendDate(); 
    }

    private void SendDate(bool display = true)
    {
      try
      {
        HistoricDateTime dateTime = new HistoricDateTime(
          DayBox.Text + " " + MonthDropDown.Text + " " + YearBox.Text + " " + ADBCDropDown.Text +
          " " + TimeBox.Text
          );
        SetDateTime(dateTime, display); 
      }
      catch
      {
        SetDateTime(DateTime, display);
      }
    }

    private void DisplayDateTime()
    {
      DayBox.Text = DateTime.TwoDigits(DateTime.Day.ToString());
      MonthDropDown.Text = HistoricDateTime.MonthText(DateTime.Month);
      long year = DateTime.Year;
      if (year < 0)
        ADBCDropDown.Text = "BC";
      else
        ADBCDropDown.Text = "AD";
      YearBox.Text = Math.Abs(year).ToString();
      TimeBox.Text = DateTime.ToTimeString();
    }

    private void SetDateTime(HistoricDateTime dateTime, bool display = true)
    {
      DateTime = dateTime;
      if (display)
      {
        DisplayDateTime(); 
      }
      CoreDll.SetDaysSinceJ2000(dateTime.TotalDays);
      RenderablesTimeUpdate(); 
    }

    private void RenderablesTimeUpdate()
    {
      foreach (IRenderable renderable in Scene.RenderableObjects)
        renderable.TimeUpdate(); 
    }

    private void DayBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        SendDate(); 
    }

    private void ADBCToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetADBC((sender as ToolStripMenuItem).Text);
    }

    private void TimeLockButton_Click(object sender, EventArgs e)
    {
      SetTimeLock(!TimeUnlocked);
    }

    private void SetTimeLock(bool value)
    {
      TimeUnlocked = value;
      DayBox.Enabled = MonthDropDown.Enabled = YearBox.Enabled = ADBCDropDown.Enabled = TimeBox.Enabled = TimeUnlocked;
      if (TimeUnlocked)
        TimeLockButton.Image = Properties.Resources.TimeIcon;
      else
        TimeLockButton.Image = Properties.Resources.TimeLockIcon;
    }

    private void TestTriadButton_Click(object sender, EventArgs e)
    {
      Mesh arrow = TriadGeometry.GenerateArrow();
      Mesh childArrow = TriadGeometry.GenerateArrow();
      childArrow.SetColor(new ColorFloat(1, 1, 0));
      arrow.Children.Add(childArrow); 
      Mesh xTriangle = TriadGeometry.XTriangle;
      
      Scene.RenderableObjects.Add(new TriadGeometry().Arrows);
      Scene.RenderableObjects.Add(arrow);
      Scene.RenderableObjects.Add(xTriangle); 

      Debugging.RotationTest test = new Debugging.RotationTest(arrow, xTriangle)
      {
        Owner = this
      };

      test.Show(); 
    }

    private void PlayPauseButton_Click(object sender, EventArgs e)
    {
      SimulationRunning = !SimulationRunning;
      if (SimulationRunning)
      {
        TimeLockButton.Enabled = false;
        InitializeMeteorShowerButton.Enabled = false; 
        DeleteMeteorShowerButton.Enabled = false; 
        SetTimeLock(false);
        SimulationWorker.DoWork -= Simulate;
        SimulationWorker.DoWork += Simulate;
        PlayPauseButton.Image = Properties.Resources.Pause;
        SimulationWorker.RunWorkerAsync();
      }
      else
      {
        PlayPauseButton.Image = Properties.Resources.Play;
        TimeLockButton.Enabled = true;
        InitializeMeteorShowerButton.Enabled = true; 
        DeleteMeteorShowerButton.Enabled = true; 
        CoreDll.Run(false); 
        while (!SimulationWorkerReady)
          System.Threading.Thread.Sleep(10); 
      }
    }

    private void Simulate(object sender, DoWorkEventArgs e)
    {
      
      SimulationWorkerReady = false;
      //while (SimulationRunning)
      //  AddTimeStep(); 
      CoreDll.Run(true); 
      CoreDll.Simulate(); 
      SimulationWorkerReady = true;       
    }

    private void SetTimeStep()
    {
      double timeStep = Convert.ToDouble(TimeStepBox.Text);
      if (timeStep <= 0)
        throw new ArgumentOutOfRangeException(); 
      switch (TimeStepUnitButton.Text)
      {
        case "Seconds":
          timeStep /= 86400;
          break;
        case "Minutes":
          timeStep /= 1440;
          break;
        case "Hours":
          timeStep /= 24;
          break;
      }
      TimeStep = timeStep; 
    }

    private void SetStepUnit(object sender, EventArgs e)
    {
      TimeStepUnitButton.Text = sender.ToString(); 
      SetTimeStep(); 
    }

    private void TimeStepBox_Leave(object sender, EventArgs e)
    {
      try
      {
        SetTimeStep();
      }
      catch
      {
        TimeStepBox.Text = "1";
        SetTimeStep(); 
      }
    }

    private void TimeStepBox_KeyDown(object sender, KeyEventArgs e)
    {
      try
      {
        SetTimeStep();
      }
      catch
      {
        TimeStepBox.Text = "1";
        SetTimeStep();
      }
    }

    private void InitializePlanets_Click(object sender, EventArgs e)
    {
      InitializePlanets(); 
    }

    private void InitializePlanets()
    { 
      //if (Earth == null)
      //{
      //  MessageBox.Show("Wait for earth to initialize first.");
      //  return; 
      //}
      InitializeEarth(null,null); 
      Sun = AddSun();
      Mercury = AddPlanet(SolarSystemPlanet.Mercury);
      Venus = AddPlanet(SolarSystemPlanet.Venus);
      Moon = AddPlanet(SolarSystemPlanet.Moon);
      Moon.SetColorMap(new ColorMap("Grayscale"));    
      Mars = AddPlanet(SolarSystemPlanet.Mars);
      Mars.SetColorMap(new ColorMap("Mars"));
      Jupiter = AddPlanet(SolarSystemPlanet.Jupiter);
      Saturn = AddPlanet(SolarSystemPlanet.Saturn);
      Uranus = AddPlanet(SolarSystemPlanet.Uranus);
      Neptune = AddPlanet(SolarSystemPlanet.Neptune);
      Pluto = AddPlanet(SolarSystemPlanet.Pluto);

      //Mercury.Position = new Point3D(Sun.MaximumRadius + 2 * Mercury.MaximumRadius);
      //Venus.Position = new Point3D(Mercury.Position.x + Venus.MaximumRadius * 2);
      //Earth.Position = new Point3D(Venus.Position.x + Earth.MaximumRadius * 2);
      //Moon.Position = new Point3D(Earth.Position.x + Earth.MaximumRadius * 2);
      //Mars.Position = new Point3D(Moon.Position.x + Mars.MaximumRadius * 2);
      //Jupiter.Position = new Point3D(Mars.Position.x + Jupiter.MaximumRadius * 1.2);
      //Saturn.Position = new Point3D(Jupiter.Position.x + Jupiter.MaximumRadius * 2);
      //Uranus.Position = new Point3D(Saturn.Position.x + Saturn.MaximumRadius * 2);
      //Neptune.Position = new Point3D(Uranus.Position.x + Uranus.MaximumRadius * 2);
      //Pluto.Position = new Point3D(Neptune.Position.x + Neptune.MaximumRadius * 2);


      Sun.AddTexture(@"Resource\Texture\8k_sun.jpg",0,false);
      Mercury.AddTexture(@"Resource\Texture\8k_mercury.jpg",0, false);
      Venus.AddTexture(@"Resource\Texture\8k_venus_surface.jpg",0, false);
      Venus.AddTexture(@"Resource\Texture\8k_venus_atmosphere.jpg",0, false);
      Earth.AddTexture(@"Resource\Texture\8k_earth_daymap.jpg", -5, false);
      Earth.AddTexture(@"Resource\Texture\8k_earth_daymap+Atmosphere.jpg", -5, false);
      Moon.AddTexture(@"Resource\Texture\8k_moon.jpg", 180, false);
      Mars.AddTexture(@"Resource\Texture\8k_mars.jpg", 180, false);
      Jupiter.AddTexture(@"Resource\Texture\8k_jupiter.jpg", 0, false);
      Saturn.AddTexture(@"Resource\Texture\8k_saturn.jpg", 0, false);
      Uranus.AddTexture(@"Resource\Texture\2k_uranus.jpg", 0, false);
      Neptune.AddTexture(@"Resource\Texture\2k_neputune.jpg", 0, false);
      Pluto.AddTexture(@"Resource\Texture\Pluto.jpg", 0, false);

      InitializeMoons();

      //if (Camera.Eye is PositionObject eye)
      //  eye.Position = new Point3D(Pluto.Position.x, Pluto.MaximumRadius * 5);

      InitializeEquatorialCoordinateSystem(); 
    }

    private void InitializeMoons()
    {
      //Jupiters moons
      Io = AddPlanet(SolarSystemPlanet.Io);
      Europa = AddPlanet(SolarSystemPlanet.Europa);
      Ganymede = AddPlanet(SolarSystemPlanet.Ganymede);
      Callisto = AddPlanet(SolarSystemPlanet.Callisto);

      Io.AddTexture(@"Resource\Texture\Europa.jpg", 0, false);
      Ganymede.AddTexture(@"Resource\Texture\ganymede.jpg", 0, false);
      Callisto.AddTexture(@"Resource\Texture\callisto.jpg", 0, false);
      Europa.AddTexture(@"Resource\Texture\Europa.jpg", 0, false);

      //Saturns moons
      Titan = AddPlanet(SolarSystemPlanet.Titan);
      Mimas = AddPlanet(SolarSystemPlanet.Mimas);
      Enceladus = AddPlanet(SolarSystemPlanet.Enceladus);
      Tethys = AddPlanet(SolarSystemPlanet.Tethys);
      Dione = AddPlanet(SolarSystemPlanet.Dione);
      Rhea = AddPlanet(SolarSystemPlanet.Rhea);
      Hyperion = AddPlanet(SolarSystemPlanet.Hyperion);
      Iapetus = AddPlanet(SolarSystemPlanet.Iapetus);

      Titan.AddTexture(@"Resource\Texture\titan.jpg", 0, false);
      Mimas.AddTexture(@"Resource\Texture\mimas.jpg", 0, false);
      Enceladus.AddTexture(@"Resource\Texture\enceladus.jpg", 0, false);
      Tethys.AddTexture(@"Resource\Texture\tethys.jpg", 0, false);
      Dione.AddTexture(@"Resource\Texture\dione.jpg", 0, false);
      Rhea.AddTexture(@"Resource\Texture\rhea.jpg", 0, false);
      //Hyperion.AddTexture(@"Resource\Texture\hyperion.jpg", 0, false);
      Iapetus.AddTexture(@"Resource\Texture\iapetus.jpg", 0, false);

      //Uranus moons
      Miranda = AddPlanet(SolarSystemPlanet.Miranda);
      Ariel = AddPlanet(SolarSystemPlanet.Ariel);
      Umbriel = AddPlanet(SolarSystemPlanet.Umbriel);
      Titania = AddPlanet(SolarSystemPlanet.Titania);
      Oberon = AddPlanet(SolarSystemPlanet.Oberon);

      Miranda.AddTexture(@"Resource\Texture\miranda.jpg", 0, false);
      Ariel.AddTexture(@"Resource\Texture\areial.jpg", 0, false);
      Umbriel.AddTexture(@"Resource\Texture\umbriel.jpg", 0, false);
      Titania.AddTexture(@"Resource\Texture\titania.jpg", 0, false);
      Oberon.AddTexture(@"Resource\Texture\oberon.jpg", 0, false);

      //Neptunes moons
      Triton = AddPlanet(SolarSystemPlanet.Triton);

      Triton.AddTexture(@"Resource\Texture\triton.jpg", 0, false);

      //Plutos moons
      Charon = AddPlanet(SolarSystemPlanet.Charon);

      Charon.AddTexture(@"Resource\Texture\charon.jpg", 0, false);
    }

    private void MaxRenderRatioBox_Click(object sender, EventArgs e)
    {
      try
      {
        Planet.MaxRenderRatio = Convert.ToDouble(MaxRenderRatioBox.Text);
      }
      catch
      {
        MaxRenderRatioBox.Text = Planet.MaxRenderRatio.ToString(); 
      }
    }

    private void InitializeEquatorialCoordinateSystem()
    {
      EquatorialCoordinateSystem equatorialCoordinateSystem = new EquatorialCoordinateSystem(Earth);

      foreach (IRenderable renderable in Scene.RenderableObjects)
        if (renderable is Planet planet)
        {
          Quaternion rotation = equatorialCoordinateSystem.PlanetQuaternion(planet.RightAscension, planet.Declination);

          planet.RotationAxis = rotation;
        }
    }

    private void TimeStepButton_Click(object sender, EventArgs e)
    {
      if (SimulationRunning)
        return;
      CoreDll.AddTimeStep(TimeStep); 
    }

    private void QualityBox_Leave(object sender, EventArgs e)
    {
      SetQuality();
    }

    private void SetQuality()
    {
      try
      {
        int quality = Convert.ToInt32(QualityBox.Text);
        if (quality < 0 || quality > 9)
          throw new Exception();
        Scene.MaximumDisplayGeneration = quality; 
      }
      catch
      {
        MessageBox.Show("Value must be between 0 and 9.");
      }
    }

    private void QualityBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        SetQuality(); 
    }

    private void CalibrationBox_TextChanged(object sender, EventArgs e)
    {
      try
      {
        double calibration = Convert.ToDouble(CalibrationBox.Text);
        if (ActiveObject is Planet planet)
        {
          planet.RotationCalibration = calibration;
          Scene.Changed = true;

          SetDateTime(DateTime);
        }
      }
      catch
      {

      }
    }

    private void InitializeMeteorShowerButton_Click(object sender, EventArgs e)
    {
      if (meteoriteInitializationForm == null || meteoriteInitializationForm.IsDisposed)
        meteoriteInitializationForm = new MeteoriteInitializationForm(Scene);

      meteoriteInitializationForm.Owner = this; 
      meteoriteInitializationForm.Show(); 
    }

    private void DeleteMeteorShowerButton_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < Scene.RenderableObjects.Count; i++)
      {
        IRenderable renderable = Scene.RenderableObjects[i];
        if (renderable is MeteorShower meteorShower)
        {
          Scene.RenderableObjects.RemoveAt(i); 
          meteorShower.Dispose();
          i--; 
        }
      }
    }

    private void LoadTextureMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        int index = SceneContentBox.SelectedIndex;
        if (index < 0 || index >= Scene.RenderableObjects.Count)
          throw new Exception("Please select a planet in the list.");
        
        Planet planet = Scene.RenderableObjects[SceneContentBox.SelectedIndex] as Planet;
        if (planet == null)
          throw new Exception("Please select a planet in the list.");
        
        using (OpenFileDialog ofd = new OpenFileDialog())
        {
          if (ofd.ShowDialog() != DialogResult.OK)
            return;
          using (Image image = Image.FromFile(ofd.FileName))
          {

          }
          string rotationText = Prompt.ShowDialog("Rotation of image in degrees.", "Rotation");
          double rotation = 0;
          try
           {
            rotation = Convert.ToDouble(rotationText);
          }
          catch
          {

          }

          planet.AddTexture(ofd.FileName, rotation);

        }
      }

      catch (Exception ex)
      {
        MessageBox.Show("Cannot Apply Texture.\n" + ex.Message, "Error");
      }

    }
  }
}
