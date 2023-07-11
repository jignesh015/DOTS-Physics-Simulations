using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities.UniversalDelegates;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VoxelBlockSystem : ISystem
    {
        //[BurstCompile]
        //public void OnUpdate(ref SystemState state)
        //{
        //    EntityManager em = state.EntityManager;

        //    //Get all voxel blocks
        //    var voxelBlocksEntities = state.GetEntityQuery(new ComponentType[] { typeof(VoxelBlock) }).ToEntityArray(Allocator.TempJob);
        //    foreach( var voxelBlockEntity in voxelBlocksEntities )
        //    {
        //        VoxelBlock voxelBlock = em.GetComponentData<VoxelBlock>(voxelBlockEntity);
        //        if (!voxelBlock.IsBlockComplete)
        //        {
        //            var voxelBlockTransform = em.GetComponentData<LocalTransform>(voxelBlockEntity);

        //            ////Spawn voxels as per row, column and height
        //            //for (int i = 0; i < voxelBlock.MaxHeight; i++)
        //            //{
        //            //    Entity voxelEntity = em.Instantiate(voxelBlock.VoxelPrefab);
        //            //    var voxelTransform = em.GetComponentData<LocalTransform>(voxelEntity);
        //            //    voxelTransform.Position = new float3(voxelBlockTransform.Position.x, voxelBlock.HeightOffset * i, voxelBlockTransform.Position.z);

        //            //    Voxel voxel = em.GetComponentData<Voxel>(voxelEntity);
        //            //    voxel.Row = voxelBlock.Row;
        //            //    voxel.Column = voxelBlock.Column;
        //            //    voxel.Height = i;

        //            //    em.SetComponentData(voxelEntity, voxelTransform);
        //            //    em.SetComponentData(voxelEntity, voxel);
        //            //}
        //            //voxelBlock.IsBlockComplete = true;
        //            //em.SetComponentData(voxelBlockEntity, voxelBlock);

        //            Entity voxelEntity = em.Instantiate(voxelBlock.VoxelPrefab);
        //            var voxelTransform = em.GetComponentData<LocalTransform>(voxelEntity);
        //            voxelTransform.Position = voxelBlockTransform.Position;
        //            em.SetComponentData(voxelEntity, voxelTransform);

        //            Voxel voxel = em.GetComponentData<Voxel>(voxelEntity);
        //            voxel.Row = voxelBlock.Row;
        //            voxel.Column = voxelBlock.Column;
        //            voxel.Height = voxelBlock.Height;
        //            voxel.IsVoxelReady = false;
        //            em.SetComponentData(voxelEntity, voxel);

        //            voxelBlock.IsBlockComplete = true;
        //            em.SetComponentData(voxelBlockEntity, voxelBlock);
        //        }
        //    }
        //}
    }
}
