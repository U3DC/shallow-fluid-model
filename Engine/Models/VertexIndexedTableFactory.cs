﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Polyhedra;
using Engine.Utilities;
using MathNet.Numerics.LinearAlgebra;

namespace Engine.Models
{
    /// <summary>
    /// Static methods for constructing fast-access tables to geometric information about a surface.
    /// </summary>
    public static class VertexIndexedTableFactory
    {
        /// <summary>
        /// Constructs a table of the neighbours of each vertex.
        /// </summary>
        public static int[][] Neighbours(IPolyhedron surface)
        {
            var neighbours = new int[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                var indicesOfNeighbours = surface.NeighboursOf(vertex).Select(neighbour => surface.IndexOf(neighbour));
                neighbours[surface.IndexOf(vertex)] = indicesOfNeighbours.ToArray();
            }

            return neighbours;
        }

        /// <summary>
        /// Constructs a table of the vectors perpendicular to a vertex and normal to each adjoining edge.
        /// 
        /// Normals point anticlockwise around the vertex.
        /// </summary>
        public static Vector[][] EdgeNormals(IPolyhedron surface)
        {
            var edgeNormalsTable = new Vector[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                var vertexVector = vertex.Position;
                var edgeVectors = surface.NeighboursOf(vertex).Select(neighbour => (neighbour.Position - vertexVector));
                var edgeNormals = edgeVectors.Select(edgeVector => Vector.CrossProduct(vertexVector, edgeVector).Normalize()).ToArray();
                edgeNormalsTable[surface.IndexOf(vertex)] = edgeNormals;
            }

            return edgeNormalsTable;
        }

        /// <summary>
        /// Constructs a table of the distances from each vertex to the bisectors running across each neighbouring edge.
        /// </summary>
        public static double[][] HalfEdgeLengths(IPolyhedron surface)
        {
            var halfLengthsTable = new double[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                var edges = surface.EdgesOf(vertex);
                var lengths = new List<double>();
                foreach (var edge in edges)
                {
                    var neighbour = edge.Vertices().First(v => v != vertex);
                    var bisectionPoint = surface.BisectionPoint(edge);
                    var length = (neighbour.Position - bisectionPoint).Norm();
                    //TODO: This is planar distance, not geodesic.
                    lengths.Add(length);
                }
                halfLengthsTable[surface.IndexOf(vertex)] = lengths.ToArray();
            }

            return halfLengthsTable;
        }

        /// <summary>
        /// Constructs a table of the spherical distances from each vertex to its neighbours.
        /// </summary>
        public static double[][] Distances(IPolyhedron surface)
        {
            var distanceTable = new double[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                var edges = surface.EdgesOf(vertex);
                var distances = edges.Select(edge => edge.Length()).ToArray();
                distanceTable[surface.IndexOf(vertex)] = distances;
            }

            return distanceTable;
        }

        /// <summary>
        /// Constructs a table of the faces around each vertex.
        /// 
        /// The ith edge given by surface.EdgesOf is anticlockwise of the ith face. 
        /// </summary>
        public static int[][] Faces(IPolyhedron surface)
        {
            var faceTable = new int[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                faceTable[surface.IndexOf(vertex)] = FacesAroundVertex(vertex, surface);
            }

            return faceTable;
        }

        private static int[] FacesAroundVertex(Vertex vertex, IPolyhedron surface)
        {
            var edges = surface.EdgesOf(vertex);

            var faces = new List<int>();
            for (int i = 0; i < edges.Count; i++)
            {
                var previousEdge = edges.AtCyclicIndex(i-1);
                var thisEdge = edges[i];
                var faceInCommon = surface.FacesOf(previousEdge).Intersect(surface.FacesOf(thisEdge)).First();
                var indexOfFace = surface.IndexOf(faceInCommon);
                faces.Add(indexOfFace);
            }

            return faces.ToArray();
        }

        public static Vector[] Normals(IPolyhedron surface)
        {
            var normals = new Vector[surface.Vertices.Count];
            foreach (var vertex in surface.Vertices)
            {
                var normal = vertex.Position.Normalize();
                normals[surface.IndexOf(vertex)] = normal;
            }

            return normals;
        }

        #region AreaInEachFace methods
        /// <summary>
        /// Constructs a table of the area of intersection between each vertex and the faces around it.
        /// </summary>
        public static double[][] AreaInEachFace(IPolyhedron surface)
        {
            var allAreas = new double[surface.Vertices.Count][];
            foreach (var vertex in surface.Vertices)
            {
                var faces = surface.FacesOf(vertex);
                var areas = new double[faces.Count];
                for (int index = 0; index < faces.Count; index++)
                {
                    areas[index] = AreaSharedByVertexAndFace(surface, vertex, index);
                }

                allAreas[surface.IndexOf(vertex)] = areas;
            }

            return allAreas;
        }

        private static double AreaSharedByVertexAndFace(IPolyhedron surface, Vertex vertex, int index)
        {
            var vertexPosition = vertex.Position;
            var edges = surface.EdgesOf(vertex);

            var face = surface.FacesOf(vertex)[index];
            var midpointOfFace = face.Center();

            var previousEdge = edges.AtCyclicIndex(index - 1);         
            var midpointOfPreviousEdge = surface.BisectionPoint(previousEdge);

            var nextEdge = edges.AtCyclicIndex(index);
            var midpointOfNextEdge = surface.BisectionPoint(nextEdge);

            var crossProductOfFirstSegment = Vector.CrossProduct(midpointOfPreviousEdge - vertexPosition, midpointOfFace - vertexPosition);
            var areaOfFirstSegment = Vector.ScalarProduct(crossProductOfFirstSegment, midpointOfFace) / 2;

            var crossProductOfSecondSegment = Vector.CrossProduct(midpointOfFace - vertexPosition, midpointOfNextEdge - vertexPosition);
            var areaOfSecondSegment = Vector.ScalarProduct(crossProductOfSecondSegment, midpointOfFace) / 2;

            return areaOfFirstSegment + areaOfSecondSegment;
        }
        #endregion


        /// <summary>
        /// Constructs a table of the spherical areas associated with each vertex. 
        /// 
        /// Only valid for degree-3 vertices.
        /// </summary>
        public static double[] Areas(IPolyhedron surface)
        {
            var areasInEachFace = AreaInEachFace(surface);
            var areas = new double[surface.Vertices.Count];
            foreach (var vertex in surface.Vertices)
            {
                var vertexIndex = surface.IndexOf(vertex);
                areas[vertexIndex] = areasInEachFace[vertexIndex].Sum();
            }

            return areas;
        }
    }
}
