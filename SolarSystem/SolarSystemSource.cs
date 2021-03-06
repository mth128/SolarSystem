﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem
{
  public class SolarSystemSource
  {
    public List<PlanetProperties> PlanetProperties { get; } = new List<PlanetProperties>(); 
    public SolarSystemSource(string fileName)
    {
      BasicCsvReader reader = new BasicCsvReader(fileName);
      for (int i = 0; i < reader.RowCount; i++)
        PlanetProperties.Add(new PlanetProperties(reader, i));
    }
  }

  public class PlanetProperties
  {
    private bool formatWarned = false; 
    public string Name { get; }
    public double EquatorialRadius { get; }
    public double PolarRadius { get; }
    public double SurfaceGravity { get; }
    public double SemiMajorAxis { get; }
    public double OrbitalEccentricity { get; }
    public double OrbitalInclination { get; }
    public double SiderealOrbitPeriod { get; }
    public double SiderealRotationPeriod { get; }
    public double LongitudeOfAscendingNode { get; }
    public double LongitudeOfPeriapsis { get; }
    public double RightAscension { get; }
    public double Declination { get; }
    public HistoricDateTime TimeOfPeriapsis { get; }
    public float R { get; }
    public float G { get; }
    public float B { get; }
    public string Texture { get; }
    public double Calibration { get; }
    public double TextureRotation { get; }
    public string IsMoonOf { get; }
    public bool SynchronousRotation { get; }
    public bool IsSun { get; }

    public double SemiMinorAxis => SemiMajorAxis * Math.Sqrt(1 - OrbitalEccentricity * OrbitalEccentricity);
    public double FocalDistance => Math.Sqrt((SemiMajorAxis * SemiMajorAxis) - (SemiMinorAxis * SemiMinorAxis));
    public double Apoapsis => SemiMajorAxis + FocalDistance;
    public double Periapsis => SemiMajorAxis - FocalDistance; 

    public double RingRadius { get; }
    public string RingTexture { get; }
    public double RingTextureInnerRadius { get; }

    public PlanetProperties(BasicCsvReader reader, int index)
    {    
      Name = reader.GetString("Name", index);
      EquatorialRadius = reader.GetDouble("EquatorialRadius", index);
      PolarRadius = reader.GetDouble("PolarRadius", index);
      SurfaceGravity = reader.GetDouble("SurfaceGravity", index);
      SemiMajorAxis = reader.GetDouble("SemiMajorAxis", index);
      OrbitalEccentricity = reader.GetDouble("OrbitalEccentricity", index);
      OrbitalInclination = reader.GetDouble("OrbitalInclination", index);
      SiderealRotationPeriod = reader.GetDouble("SiderealRotationPeriod", index);
      SiderealOrbitPeriod = reader.GetDouble("SiderealOrbitPeriod", index);
      LongitudeOfAscendingNode = reader.GetDouble("LongitudeOfAscendingNode", index);
      LongitudeOfPeriapsis = reader.GetDouble("LongitudeOfPeriapsis", index);
      RightAscension = reader.GetDouble("RightAscension", index);
      Declination = reader.GetDouble("Declination", index);

      try
      {
        string date = reader.GetString("TimeOfPeriapsis", index);
        if (date != "")
          TimeOfPeriapsis = new HistoricDateTime(date);
        else
          TimeOfPeriapsis = new HistoricDateTime();
      }
      catch
      {
        if (!formatWarned)
        {
          formatWarned = true;
          System.Windows.Forms.MessageBox.Show("Please provide date in form " + new HistoricDateTime().ToString());
        }
        TimeOfPeriapsis = new HistoricDateTime();
      }

      R = reader.GetFloat("R",index);
      G = reader.GetFloat("G",index);
      B = reader.GetFloat("B",index);
      if (R>1 || G>1 || B>1)
      {
        R /= 255;
        G /= 255;
        B /= 255; 
      }
      Texture = reader.GetString("Texture", index);
      Calibration = reader.GetDouble("Calibration", index);
      TextureRotation = reader.GetDouble("TextureRotation", index); 
      IsMoonOf = reader.GetString("IsMoonOf", index);
      SynchronousRotation = reader.GetBool("SynchronousRotation", index);
      IsSun = reader.GetBool("IsSun", index);

      RingRadius = reader.GetDouble("RingRadius", index);
      RingTexture = reader.GetString("RingTexture", index);
      RingTextureInnerRadius = reader.GetDouble("RingTextureInnerRadius", index); 
    }

    public void AddToScene(Scene scene, Camera camera)
    {
      int planetID = CoreDll.AddPlanet(Name, EquatorialRadius, PolarRadius,
        SurfaceGravity, Apoapsis, Periapsis, OrbitalInclination, SiderealOrbitPeriod, SiderealRotationPeriod, 
        LongitudeOfAscendingNode, LongitudeOfPeriapsis, RightAscension, Declination, TimeOfPeriapsis.TotalDays, 
        SynchronousRotation, IsMoonOf, IsSun, R, G, B) ;

      List<Planet> planets = scene.GetPlanets();
      Planet planet = null;
      foreach (Planet existingPlanet in planets)
      {
        if (existingPlanet.Name == Name)
        {
          planet = existingPlanet;
          planet.ID = planetID;
          planet.RotationCalibration = Calibration; 
        }
      }
      if (planet == null)
        planet = new Planet(planetID, Name, Calibration);

      if (IsSun)
      {
        planet.RenderableObject.UseLight = false;
        scene.SunLight = new SunLight(planet);
        scene.Lights.Add(scene.SunLight);
        if (camera!=null)
          camera.Light.On = false;
      }

      string colorMapName = @"Resource\" + planet.Name + ".cmap";
      string defaultColorMap = @"Resource\default.cmap";

      if (System.IO.File.Exists(colorMapName))
        planet.SetColorMap(new ColorMap(colorMapName,true));
      else
        planet.SetColorMap(new ColorMap(defaultColorMap, true));

      foreach (string texture in Texture.Split(','))
        planet.AddTexture(texture, TextureRotation, false);

      if (EquatorialCoordinateSystem.Main!=null)
        planet.RotationAxis = EquatorialCoordinateSystem.Main.PlanetQuaternion(planet.RightAscension, planet.Declination);

      foreach (Planet existingPlanet in scene.GetPlanets())
        if (existingPlanet.ID == planetID)
          return;

      scene.RenderableObjects.Add(planet); 

      if (RingRadius!=0)
        planet.RingSystem = new PlanetaryRingSystem(planet, RingRadius, RingTexture, RingTextureInnerRadius);
      

    }
  }
}
