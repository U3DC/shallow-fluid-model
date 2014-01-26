﻿using Assets;
using Assets.Rendering;
using Engine.Icosasphere;
using Engine.Polyhedra;
using UnityEngine;
using System.Collections;

public class testHook : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    Polyhedron polyhedron = new Icosahedron();
        polyhedron = IcosasphereSubdivider.Subdivide(polyhedron);
        polyhedron = IcosasphereSubdivider.Subdivide(polyhedron);
        polyhedron = IcosasphereSubdivider.Subdivide(polyhedron);
        polyhedron = IcosasphereSubdivider.Subdivide(polyhedron);
        polyhedron = IcosasphereSubdivider.Subdivide(polyhedron);
        //new PolyhedronRenderer(polyhedron, "Test", "Materials/TestMaterial", "Materials/TestMaterial2");
        Debug.Log(polyhedron.Vertices.Count);
        var polyhedron2 = new Geodesic(polyhedron);
        new PolyhedronRenderer(polyhedron2, "Test2", "Materials/TestMaterial", "Materials/TestMaterial2");
        Debug.Log(polyhedron2.Vertices.Count);
	}
}
