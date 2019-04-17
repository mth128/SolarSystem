#pragma once
#include "Point3D.h"
#include "Color.h"
//Copyright Maarten 't Hart 2019
#include <vector>
#include <ctime>

#define CTime tm

//time related issues. Time is in seconds unless specified otherwise. 
CTime Time(double Year, double month, double day, double hour, double minute, double second);
double TimeSinceJ2000(CTime time);
double TimeSinceJ2000(double year, double month, double day, double hour, double minute, double second);
double DaysSinceJ2000(double year, double month, double day, double hour, double minute, double second);

struct Orbit
{
	//http://en.wikipedia.org/wiki/Orbital_inclination
	//
	double epoch;//base time: J2000: 12:00 (UMT) 1 january 2000. 
	double apoapsis;//(km) largest distance to orbitting object. Also called aphelium for the sun.
	double periapsis;//(km) smallest distance to orbitting object. Also called perihelium for the sun.
	double semimajorAxis;//km
	double eccentricity;
	double inclination;//degrees, to invariable plane

	double inclinationRad, longitudeAscendingNodeRad, argumentPeriapsisRad;

	double longitudeAscendingNode;//degrees
	double argumentPeriapsis;//degrees
	double period;//days
	double averageSpeed;//km/second
	
	double timeOfPeriapsis;//time at which the planet was at the pseriapsis (in days). 
	
	//variables for fast calculating of position:
	double focusDistance;
	double relativeFocusDistance;
	double semiminorAxis;
	double flatFactor;

	Orbit();//default is the values for earth;
	
	//position in space based on 2D position.
  //this formula uses the reference direction 1,0,0
	Point3D Position(const Point3D&ellipsePosition) const;
	Point3D Position(double t) const;//position in space based on t-value in kilometers. 
	Point3D PositionByTime(double days) const;//position in space based on time. Time is in days. 
	double EllipsePosition(double days) const;
	void SetCalculationValues();
};

struct CelestialBody
{
	double	Mass;
	double	Volume;
	double	EquatorialRadius;
	double	PolarRadius;
	double	VolumetricMeanRadius;
	double	Ellipticity;
	double	MeanDensity;
	double	SurfaceGravity;
	double	SurfaceAccelleration;
	double	EscapeVelocity;
	double	GM;

	double	SemiMajorAxis;
	double	SiderealOrbitPeriod;
	double	TropicalOrbitPeriod;
	double	Periapsis;
	double	Apoapsis;
	double	SynodicPeriod;
	double	MeanOrbitalVelocity;
	double	MaxOrbitalVelocity;
	double	MinOrbitalVelocity;
	double	OrbitalInclination;
	double	OrbitalEccentricity;
	double	SiderealRotationPeriod;
	double	LenghtOfDay;
	double	ObliquityToOrbit;

	double	LongitudeOfAscendingNode;
	double	LongitudeOfPeriapsis;
	double	MeanLongitude;

	double	RightAscension;
	double	Declination;
	double	TimeOfPeriapsis;

	Color color;

	void SetMercury();
	void SetVenus();
	void SetEarth();
	void SetMoon();
	void SetMars();
	void SetJupiter();
	void SetSaturn();
	void SetUranus();
	void SetNeptune();
	void SetPluto();
	void SetSun();
};

struct Planet : CelestialBody, Orbit
{
	std::string name;

	double radius;//km
	double secondaryRadius;//km
	double rotationTime;//hours per turn
	double surfaceGravity;//km/s2

	bool isSun;
	bool isMoon;
	Point3D position;

	Planet(double radius, double secondaryRadius, double surfaceGravity, std::string name = "");
	void LoadCelestialBodyOrbit();//puts the nasa data in the orbit struct. 
};

