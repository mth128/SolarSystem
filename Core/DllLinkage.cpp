//Copyright Maarten 't Hart 2019
#include "stdafx.h"
#include "DllLinkage.h"
#include <string>
#include "SolarSystem.h"
#include "Geodesic.h"


//Tests and Examples. 
void ExampleSetString(const char*theString)
{
	std::string example(theString);
}

int ExampleGetInt()
{
	int a = 40;
	int b = 2;
	int c = a + b;
	return c;
}

std::vector<double> testVertices{
	-1, 0.5, 0,
		1, 0.5, 0,
		0, -1, 0,
		0, 0, 1 
};

std::vector<int> testIndices
{
	2, 1, 0,
		3, 0, 1,
		3, 1, 2,
		3, 2, 0
};

double* TestVertices(int generation)
{
	return &testVertices[0];
}

int TestVerticesCount(int generation)
{
	return testVertices.size(); 
}

const int* TestIndices(int generation)
{
	return &testIndices[0];
}

int TestIndicesCount(int generation)
{
	return testIndices.size(); 
}
//End of Tests and Examples. 


const double* GeodesicGridVertices(int generation)
{
	const GeodesicGrid*grid = GetGeodesicGrid((unsigned int)generation);
	return grid->points[0].XYZ;
}

int GeodesicGridVerticesCount(int generation)
{
	const GeodesicGrid*grid = GetGeodesicGrid((unsigned int)generation);
	return grid->points.size(); 
}

const int* GeodesicGridIndices(int generation)
{
	const GeodesicGrid*grid = GetGeodesicGrid((unsigned int)generation);
	return grid->GetTriangleIndices()[0].i;
}

int GeodesicGridIndicesCount(int generation)
{
	const GeodesicGrid*grid = GetGeodesicGrid((unsigned int)generation);
	return grid->GetTriangleIndices().size()*3; 
}