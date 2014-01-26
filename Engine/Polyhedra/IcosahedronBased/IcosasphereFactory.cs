﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Utilities;
using MathNet.Numerics;

namespace Engine.Polyhedra.IcosahedronBased
{
    public class IcosasphereFactory
    {
        /// <summary>
        /// Constructs the icosasphere with least number of vertices exceeding the specified minimum. 
        /// </summary>
        public static Polyhedron Build(int minimumNumberOfVertices)
        {
            var icosahedron = IcosahedronFactory.Build();
            var numberOfSubdivisions = NumberOfSubdivisionsRequiredForVertexCount(minimumNumberOfVertices);

            for (int i = 0; i < numberOfSubdivisions; i++)
            {
                icosahedron = Subdivide(icosahedron);
            }

            return ProjectOntoSphere(icosahedron);
        }

        private static double NumberOfSubdivisionsRequiredForVertexCount(int minimumNumberOfVertices)
        {
            var vertices = 12;
            var edges = 30;
            var faces = 20;

            var subdivisions = 0;
            while (vertices < minimumNumberOfVertices)
            {
                vertices = vertices + edges;
                edges = 2*edges + 3*faces;
                faces = 4*faces;
                
                subdivisions = subdivisions + 1;
            }

            return subdivisions;
        }

        #region Subdivision methods.
        private static Polyhedron Subdivide(Polyhedron icosasphere)
        {
            var oldEdgesToNewVertices = CreateNewVerticesFrom(icosasphere.Edges);
            var newFaces = CreateFacesFrom(icosasphere.Faces, icosasphere.FaceToEdges, oldEdgesToNewVertices);

            return new Polyhedron(newFaces);
        }

        private static IEnumerable<IEnumerable<Vertex>> CreateFacesFrom
            (List<Face> faces, Dictionary<Face, HashSet<Edge>> oldFacesToOldEdges, Dictionary<Edge, Vertex> oldEdgesToNewVertices)
        {
            var newFaces = new List<IEnumerable<Vertex>>();
            foreach (var face in faces)
            {
                newFaces.AddRange(CreateNewFacesFrom(face, oldFacesToOldEdges, oldEdgesToNewVertices));
            }

            return newFaces;
        }

        private static IEnumerable<IEnumerable<Vertex>> CreateNewFacesFrom
            (Face oldFace, Dictionary<Face, HashSet<Edge>> oldFacesToOldEdges, Dictionary<Edge, Vertex> oldEdgesToNewVertices)
        {
            var newFaces = new List<IEnumerable<Vertex>>();

            var edges = oldFacesToOldEdges[oldFace];
            foreach (var vertex in oldFace.Vertices)
            {
                var adjacentEdges = edges.Where(edge => edge.A == vertex || edge.B == vertex);
                var newVertices = adjacentEdges.Select(edge => oldEdgesToNewVertices[edge]).ToList();
                newVertices.Add(vertex);
                newFaces.Add(newVertices);
            }
            var centralFace = edges.Select(edge => oldEdgesToNewVertices[edge]).ToList();
            newFaces.Add(centralFace);

            return newFaces;
        }

        private static Dictionary<Edge, Vertex> CreateNewVerticesFrom(List<Edge> edges)
        {
            return edges.Distinct().ToDictionary(edge => edge, edge => VertexAtMidpointOf(edge));
        }

        private static Vertex VertexAtMidpointOf(Edge edge)
        {
            var position = (edge.A.Position + edge.B.Position) / 2;

            return new Vertex(position);
        }
        #endregion

        private static Polyhedron ProjectOntoSphere(Polyhedron polyhedron)
        {
            var newVertex = 
                polyhedron.Vertices.
                ToDictionary(oldVertex => oldVertex, oldVertex => new Vertex(oldVertex.Position.Normalize()));

            var newFaces =
                from face in polyhedron.Faces
                select face.Vertices.Select(oldVertex => newVertex[oldVertex]);

            return new Polyhedron(newFaces);
        }
    }
}
