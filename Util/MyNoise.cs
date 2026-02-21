using MultiplayerVoxelGame.Game.Client.World.WorldGeneration;
using NoiseDotNet;
using System.Buffers;

namespace MultiplayerVoxelGame.Util
{
    public static class MyNoise
    {

        public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
        {
            return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
        }

        public static float RemapValue01(float value, float outputMin, float outputMax)
        {
            return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
        }

        public static int RemapValue01ToInt(float value, float outputMin, float outputMax)
        {
            return (int)RemapValue01(value, outputMin, outputMax);
        }

        public static float Redistribution(float noise, NoiseSettings settings)
        {
            return MathF.Pow(noise * settings.redistributionModifier, settings.exponent);
        }

        public static void OctaveGradientNoise3D(
            Span<float> xCoords, Span<float> yCoords, Span<float> zCoords, Span<float> output,
            NoiseSettings settings, int seed)
        {

            output.Clear();

            float[] tempArray = ArrayPool<float>.Shared.Rent(output.Length);

            try
            {
                Span<float> tempSpan = tempArray.AsSpan(0, output.Length);

                float frequency = 1f;
                float amplitude = 1f;
                float amplitudeSum = 0f;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float currentXFreq = settings.noiseZoom * frequency;
                    float currentYFreq = settings.noiseZoom * frequency;
                    float currentZFreq = settings.noiseZoom * frequency;

                    Noise.GradientNoise3D(
                        xCoords, yCoords, zCoords, tempSpan,
                        currentXFreq, currentYFreq, currentZFreq, amplitude, seed + i
                    );

                    for (int j = 0; j < output.Length; j++)
                    {
                        output[j] += tempSpan[j];
                    }

                    amplitudeSum += amplitude;
                    amplitude *= settings.persistance;
                    frequency *= 2f;
                }

                for (int j = 0; j < output.Length; j++)
                {
                    output[j] /= amplitudeSum;
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(tempArray);
            }
        }
    }
}
