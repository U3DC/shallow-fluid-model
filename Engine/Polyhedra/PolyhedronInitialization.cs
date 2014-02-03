﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Utilities;
using MathNet.Numerics.LinearAlgebra;

namespace Engine.Polyhedra
{
    public static class PolyhedronInitialization
    {
        public static List<Vertex> Vertices(List<List<Vertex>> vertexLists)
        {
            var vertices = vertexLists.SelectMany(list => list).Distinct().ToList();

            return vertices;
        }

        #region InitializeEdges methods
        public static List<Edge> Edges(List<Face> faces)
        {
            var edges = faces.SelectMany(face => EdgesAroundFace(face)).Distinct().ToList();

            return edges;
        }

        private static IEnumerable<Edge> EdgesAroundFace(Face face)
        {
            var vertices = face.Vertices;

            var edges = new List<Edge>();
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                edges.Add(new Edge(vertices[i], vertices[i + 1]));
            }
            edges.Add(new Edge(vertices[vertices.Count - 1], vertices[0]));

            return edges;
        }
        #endregion

        public static List<Face> Faces(List<List<Vertex>> vertexLists)
        {
            var faces = vertexLists.Select(vertexList => new Face(vertexList)).ToList();

            return faces;
        }

        #region BuildDictionary methods
        public static Dictionary<Vertex, List<Edge>> VertexToEdgeDictionary(List<Vertex> vertices, List<Edge> edges)
        {
            var vertexToEdges = vertices.ToDictionary(vertex => vertex, vertex => new List<Edge>());
            foreach (var edge in edges)
            {
                vertexToEdges[edge.A].Add(edge);
                vertexToEdges[edge.B].Add(edge);
            }

            foreach (var vertex in vertices)
            {
                var comparer = new AnticlockwiseComparer(vertex.Position, -vertex.Position);
                var sortedEdges = vertexToEdges[vertex].OrderBy(edge => edge.SphericalCenter(), comparer);
                vertexToEdges[vertex] = sortedEdges.ToList();
            }

            return vertexToEdges;
        }

        public static Dictionary<Vertex, List<Face>> 
            VertexToFaceDictionary(List<Vertex> vertices, List<Face> faces, Dictionary<Vertex, List<Edge>> vertexToEdges)
        {
            var vertexToFaces = vertices.ToDictionary(vertex => vertex, vertex => new List<Face>());
            foreach (var face in faces)
            {
                foreach (var vertex in face.Vertices)
                {
                    vertexToFaces[vertex].Add(face);
                }
            }

            foreach (var vertex in vertices)
            {
                var edgesAroundVertex = vertexToEdges[vertex];
                var facesAroundVertex = vertexToFaces[vertex];

                vertexToFaces[vertex] = SortFacesToMatchEdgeOrder(vertex, edgesAroundVertex, facesAroundVertex);
            }

            return vertexToFaces;
        }

        private static List<Face> SortFacesToMatchEdgeOrder(Vertex vertex, List<Edge> edges, List<Face> faces)
        {
            var orderedFaces = new List<Face>();
            for (int index = 0; index < edges.Count; index++)
            {
                var previousNeighbour = edges.AtCyclicIndex(index).Vertices().First(v => v != vertex);
                var nextNeighbour = edges.AtCyclicIndex(index - 1).Vertices().First(v => v != vertex);

                var faceBetween = faces.First(face => face.Vertices.Contains(previousNeighbour) && face.Vertices.Contains(nextNeighbour));
                orderedFaces.Add(faceBetween);
            }

            return orderedFaces;
        }

        #region FaceToEdgeDictionary methods
        public static Dictionary<Face, List<Edge>> FaceToEdgeDictionary(List<Face> faces, Func<Vertex, List<Edge>> edgesOf)
        {
            var faceToEdges = new Dictionary<Face, List<Edge>>();
            foreach (var face in faces)
            {
                faceToEdges.Add(face, EdgesOfFace(face, edgesOf));
            }
            return faceToEdges;
        }

        private static List<Edge> EdgesOfFace(Face face, Func<Vertex, List<Edge>> edgesOf)
        {
            var edges = new List<Edge>();
            var vertices = face.Vertices;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                var edge = edgesOf(vertices[i]).Intersect(edgesOf(vertices[i + 1])).Single();
                edges.Add(edge);
            }
            var lastEdge = edgesOf(vertices[vertices.Count - 1]).Intersect(edgesOf(vertices[0])).Single();
            edges.Add(lastEdge);

            return edges;
        }
        #endregion

        public static Dictionary<Edge, List<Face>> EdgeToFaceDictionary(List<Edge> edges, List<Face> faces, Func<Face, List<Edge>> edgesOf)
        {
            var edgeToFaces = edges.ToDictionary(edge => edge, edge => new List<Face>());
            foreach (var face in faces)
            {
                foreach (var edge in edgesOf(face))
                {
                    edgeToFaces[edge].Add(face);
                }
            }
            return edgeToFaces;
        }
        #endregion

        public static Dictionary<T, int> ItemToIndexDictionary<T>(IEnumerable<T> items)
        {
            var itemList = items.ToList();
            var indices = Enumerable.Range(0, itemList.Count);
            var itemIndices = indices.ToDictionary(i => itemList[i], i => i);

            return itemIndices;
        }
    }
}
