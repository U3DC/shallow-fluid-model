﻿using Assets.Controllers;
using Assets.Options;
using Engine.Simulation.Initialization;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace Assets
{
    public class LoadingHook : MonoBehaviour
    {
        private readonly GameOptions _options = new GameOptions
        {
            MinimumNumberOfFaces = 500,
            Radius = 6000,

            Gravity = 10.0 / 1000.0,
            RotationFrequency = 1.0 / (3600.0 * 24.0),
            Timestep = 400,

            InitialHeightFunction = ScalarFieldFactory.RandomScalarField,
            InitialAverageHeight = 8,
            InitialMaxDeviationOfHeight = 2,

            InitialVelocityFunction = VectorFieldFactory.ConstantVectorField,
            InitialAverageVelocity = new Vector(new[] { 0.0, 0.0, 0.0}),
            InitialMaxDeviationOfVelocity = 0,

            ColorMapHistoryLength = 1000,
            ColorMapMaterialName = "Materials/Surface",

            ParticleCount = 20000,
            ParticleSpeedScaleFactor = 10,
            ParticleLifespan = 1000,
            ParticleTrailLifespan = 10,
            ParticleMaterialName = "Materials/ParticleMap",

            SurfaceRaiseKey = KeyCode.UpArrow,
            SurfaceLowerKey = KeyCode.DownArrow,
            IntensityIncreaseKey = KeyCode.RightBracket,
            IntensityDecreaseKey = KeyCode.LeftBracket,
            RadiusDecreaseKey = KeyCode.LeftArrow,
            RadiusIncreaseKey = KeyCode.RightArrow,

            PauseSimulationKey = KeyCode.P,
            ResetKey = KeyCode.R
        };

        private MainController _main;

        public void Start()
        {
            _main = new MainController(_options);
        }

        void Update()
        {
            _main.Update();
        }

        void OnGUI()
        {
            _main.UpdateGUI();
        }

        void OnApplicationQuit()
        {
            _main.Dispose();
        }
    }
}