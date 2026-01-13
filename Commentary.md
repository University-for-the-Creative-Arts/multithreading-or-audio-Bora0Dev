# Technical Commentary

This implementation demonstrates parallel pixel processing in Unity using the C# Jobs System with Burst compilation to achieve high-throughput analysis of image data. The core task is to compute the sum of the red (R) channel over an entire image. The design follows a common HPC pattern: partition work into independent chunks, compute per-chunk partial results in parallel, and reduce those results on the main thread.

## Multithreading / Jobs Implementation

We define a `SumRedJob : IJobParallelFor` with `[BurstCompile]`. Instead of assigning a single pixel to each job index (which would require an atomic add to combine results), we map each job index to a **batch**—a contiguous range of pixels determined by a configurable `BatchSize`. Each job iterates its range, accumulates into a local scalar, and writes to `PartialSums[batchIndex]`. After `Complete()`, the main thread reduces the partial sums into the final result. This approach avoids atomics and false sharing, plays nicely with Burst vectorization, and lets us tune overhead with `BatchSize`.

The image is loaded into a `Texture2D`, and we read `Color32[]` data (`RGBA32` layout). For the job, we copy into a `NativeArray<Color32>` (`Allocator.TempJob`) to enable zero-GC, `ReadOnly` access inside Burst-compiled code. We also allocate a `NativeArray<long>` for partial sums sized to the number of batches. The schedule uses `job.Schedule(batches, 1)`. Worker count is reported via `JobsUtility.JobWorkerCount` to contextualize throughput on the target machine.

## Benchmarking / Telemetry

We compare a **single-threaded** baseline implemented as an explicit C# `for` loop against the **Jobs+Burst** path. Wall time is measured with `System.Diagnostics.Stopwatch`. Telemetry is collected into a simple table and exported to CSV: `image, mode, batch_size, worker_threads, milliseconds, result`. The CSV can be consumed by spreadsheets, Python notebooks, or observability tools to create plots such as speedup vs. batch size or image size. Logging prints the final sum as a correctness check—both paths should match exactly for deterministic `Color32` data.

## Tools, Frameworks, APIs

- **Unity C# Jobs System** for safe parallelism, job scheduling, and worker management.
- **Burst** for ahead-of-time compilation and SIMD/vectorization of tight loops.
- **Unity.Collections** (`NativeArray<T>`) for low-overhead, Burst-compatible memory.
- **Unity.Mathematics** for fast, SIMD-friendly math intrinsics.
- **Texture2D.LoadImage** + `GetPixels32()` to ingest arbitrary PNG/JPG assets.

## Using Results for Optimisation

Timing data informs choices such as batch size, worker affinity, and memory layout. For example, larger batch sizes amortize scheduling overhead but can reduce load balancing on smaller images; smaller batch sizes may saturate more cores on very large images. CSV exports allow regression tracking (e.g., across Burst versions or hardware), and profiling in the Unity Profiler can expose cache effects and copy overheads. If analysis extends to multiple images or additional channels, the same pattern scales by (1) running multiple jobs concurrently, (2) adding channels to the accumulation, or (3) streaming tiles from disk while processing previous tiles, building a producer/consumer pipeline.

Optional extensions include plotting execution time series in-editor, adding a toggle for `IJobFor` vs `IJobParallelFor`, experimenting with `ScheduleParallel` in Entities/Jobs packages, or using `NativeReference<long>` (Unity 2022+) for more ergonomic reductions. The provided scaffold is intended to be minimal, robust, and easy to extend for broader image analytics workloads.

## Media Evidence

Video Link: https://youtu.be/_KCH9hlavws
