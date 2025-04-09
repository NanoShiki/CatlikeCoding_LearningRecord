using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class HashVisualization : Visualization {

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor {
        [ReadOnly]
        public NativeArray<float3x4> positions; 
        //manual vectorize, each column is a position, a "positions" includes 4 "position".

        [WriteOnly]
        public NativeArray<uint4> hashes;

        public SmallXXHash4 hash;

        public float3x4 domainTRS;
        //in order to control the hash value(such as less or more amount in one plane)
        
		public void Execute(int i) {
            float4x3 p = domainTRS.TransformVectors(transpose(positions[i]));
            //transpose the positions[i], then each row is a position, apply transform matrix.

            int4 u = (int4)floor(p.c0);
            int4 v = (int4)floor(p.c1);
            int4 w = (int4)floor(p.c2);
            //just a three dimension position spawn one hash value, but we get 4 hash value in one time by use manual vectorize position.

			hashes[i] = hash.Eat(u).Eat(v).Eat(w);
		}
    }

    static int hashesId = Shader.PropertyToID("_Hashes");

    [SerializeField]
    int seed;

    [SerializeField]
    SpaceTRS domain = new SpaceTRS{
        scale = 8f
    };

    NativeArray<uint4> hashes;

    ComputeBuffer hashesBuffer;

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {

        hashes = new NativeArray<uint4>(dataLength, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(dataLength * 4, 4);

        propertyBlock.SetBuffer(hashesId, hashesBuffer);
    }

    protected override void DisableVisualization() {
        hashes.Dispose();
        hashesBuffer.Release();
        hashesBuffer = null;
    }

    protected override void UpdateVisualization(
        NativeArray<float3x4> positions, int resolution, JobHandle handle
    ) {
        new HashJob {
            positions = positions,
            hashes = hashes,
            hash = SmallXXHash.Seed(seed),
            domainTRS = domain.Matrix
        }.ScheduleParallel(hashes.Length, resolution, handle).Complete();

        hashesBuffer.SetData(hashes.Reinterpret<uint>(4 * 4));
        }
    }
