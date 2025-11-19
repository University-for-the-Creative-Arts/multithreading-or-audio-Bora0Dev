using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ImageJobs
{
    /// <summary>
    /// Computes the sum of red channel values over an image using Burst + Jobs.
    /// Strategy: split the pixel buffer into fixed-size batches; each job index processes one batch and writes a partial sum.
    /// The caller reduces the partial sums on the main thread.
    /// </summary>
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct SumRedJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> Pixels;
        [WriteOnly] public NativeArray<long> PartialSums; // one element per batch
        public int BatchSize;

        public void Execute(int batchIndex)
        {
            int start = batchIndex * BatchSize;
            int end = math.min(start + BatchSize, Pixels.Length);

            long local = 0;
            for (int i = start; i < end; i++)
            {
                local += Pixels[i].r;
            }

            PartialSums[batchIndex] = local;
        }
    }
}
