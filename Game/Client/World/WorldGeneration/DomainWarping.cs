using CubeEngine.Util;
using MultiplayerVoxelGame.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class DomainWarping
    {
        public NoiseSettings noiseDomainX, noiseDomainY;
        public int amplitudeX, amplitudeY;

        public DomainWarping(NoiseSettings noiseDomainX, NoiseSettings noiseDomainY, int amplitudeX = 80, int amplitudeY = 80)
        {
            this.noiseDomainX = noiseDomainX;
            this.noiseDomainY = noiseDomainY;
            this.amplitudeX = amplitudeX;
            this.amplitudeY = amplitudeY;
        }

        /// <summary>
        /// Processes an entire chunk of coordinates at once using SIMD-accelerated noise.
        /// </summary>
        public float[] GenerateDomainNoiseBatch(float[] xCoords, float[] zCoords, NoiseSettings settings)
        {
            int count = xCoords.Length;

            float[] offsetX = new float[count];
            float[] offsetY = new float[count];

            // We calculate all X offsets, then all Y offsets
            FillOctaveNoiseBatch(xCoords, zCoords, offsetX, noiseDomainX, amplitudeX);
            FillOctaveNoiseBatch(xCoords, zCoords, offsetY, noiseDomainY, amplitudeY);

            float[] warpedX = new float[count];
            float[] warpedZ = new float[count];
            for (int i = 0; i < count; i++)
            {
                warpedX[i] = xCoords[i] + offsetX[i];
                warpedZ[i] = zCoords[i] + offsetY[i];
            }

            float[] finalOutput = new float[count];
            FillOctaveNoiseBatch(warpedX, warpedZ, finalOutput, settings, 1f);

            return finalOutput;
        }

        private void FillOctaveNoiseBatch(float[] xIn, float[] zIn, float[] outBuffer, NoiseSettings s, float finalAmplitude)
        {
            // Internal helper to handle the octaves for the whole array
            float currentFreq = s.noiseZoom;
            float currentAmp = finalAmplitude;

            // Use a temporary buffer for each octave's contribution
            float[] octaveTemp = new float[xIn.Length];
            Array.Clear(outBuffer, 0, outBuffer.Length);

            for (int i = 0; i < s.octaves; i++)
            {
                NoiseDotNet.Noise.GradientNoise2D(
                    xIn, zIn, octaveTemp,
                    currentFreq, currentFreq, currentAmp, s.seed + i);

                for (int j = 0; j < xIn.Length; j++)
                    outBuffer[j] += octaveTemp[j];

                currentFreq *= 2f;
                currentAmp *= s.persistance;
            }
        }

        //public float GenerateDomainNoise(int x, int z, NoiseSettings defaultNoiseSettings)
        //{
        //    Vector2 domainOffset = GenerateDomainOffset(x, z);
        //    return MyNoise.OctavePerlin(x + domainOffset.X, z + domainOffset.Y, defaultNoiseSettings);
        //}

        //public Vector2 GenerateDomainOffset(int x, int z)
        //{
        //    var noiseX = MyNoise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
        //    var noiseY = MyNoise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
        //    return new Vector2(noiseX, noiseY);
        //}

        //public Vector2i GenerateDomainOffsetInt(int x, int z)
        //{
        //    Vector2 offset = GenerateDomainOffset(x, z);
        //    return new Vector2i(
        //        (int)MathF.Round(offset.X),
        //        (int)MathF.Round(offset.Y)
        //    );
        //}
    }
}
