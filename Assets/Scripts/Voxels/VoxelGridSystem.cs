using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VoxelGridSystem : ISystem
    {
        private int lengthBuffer;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(SimConfigurationController.Instance == null || 
                !SimConfigurationController.Instance.carHeightMapGenerator.HeightmapReady )
                return;

            EntityManager em = state.EntityManager;

            //Get all voxel grids
            var voxelGridEntities = state.GetEntityQuery(new ComponentType[] { typeof(VoxelGrid) }).ToEntityArray(Allocator.TempJob);

            #region SIMULATANEOUS SPAWN LOGIC
            //foreach (var voxelGridEntity in voxelGridEntities)
            //{
            //    VoxelGrid voxelGrid = em.GetComponentData<VoxelGrid>(voxelGridEntity);
            //    if (!voxelGrid.IsGridComplete)
            //    {
            //        var voxelGridTransform = em.GetComponentData<LocalTransform>(voxelGridEntity);
            //        float3 vgPos = voxelGridTransform.Position;

            //        //Spawn voxels on the grid
            //        for (int i = 0; i < voxelGrid.Width; i++)
            //        {
            //            for (int j = 0; j < voxelGrid.Length; j++)
            //            {
            //                //Set voxel position on the grid
            //                Entity voxelEntity = em.Instantiate(voxelGrid.VoxelPrefab);
            //                var voxelTransform = em.GetComponentData<LocalTransform>(voxelEntity);
            //                voxelTransform.Position = new float3(vgPos.x + voxelGrid.GridOffset * i, vgPos.y, vgPos.z + voxelGrid.GridOffset * j);

            //                Debug.Log($"Voxel Pos: {voxelTransform.Position}");

            //                //Set voxel component data
            //                Voxel voxel = em.GetComponentData<Voxel>(voxelEntity);
            //                voxel.Row = j;
            //                voxel.Column = i;
            //                voxel.Height = j < (float)voxelGrid.Length / 3 || j > (float)voxelGrid.Length * 2 / 3 ? (float)voxel.MaxHeight / 2 : voxel.MaxHeight;

            //                voxel.IsVoxelReady = false;

            //                em.SetComponentData(voxelEntity, voxelTransform);
            //                em.SetComponentData(voxelEntity, voxel);
            //            }
            //        }
            //        voxelGrid.IsGridComplete = true;
            //        em.SetComponentData(voxelGridEntity, voxelGrid);
            //    }
            //}
            #endregion

            #region ASYNCHRONOUS SPAWN LOGIC
            foreach (var voxelGridEntity in voxelGridEntities)
            {
                VoxelGrid voxelGrid = em.GetComponentData<VoxelGrid>(voxelGridEntity);
                if (!voxelGrid.IsGridComplete && lengthBuffer < voxelGrid.Length)
                {
                    var voxelGridTransform = em.GetComponentData<LocalTransform>(voxelGridEntity);
                    SpawnSingleRow(em, voxelGridEntity, voxelGrid, voxelGridTransform);
                    lengthBuffer++;
                }
                else if (lengthBuffer >= voxelGrid.Length)
                {
                    voxelGrid.IsGridComplete = true;
                    em.SetComponentData(voxelGridEntity, voxelGrid);

                    //Add physics shape and physics body to the completed grid
                    //em.AddComponent<PhysicsBodyAuthoringData>(voxelGridEntity);
                    //em.AddComponent<PhysicsShapeAuthoring>(voxelGridEntity);
                }
            }
            #endregion
        }

        public void SpawnSingleRow(EntityManager em, Entity voxelGridEntity, VoxelGrid voxelGrid, LocalTransform voxelGridTransform) 
        {
            float3 vgPos = voxelGridTransform.Position;
            for (int i = 0; i < voxelGrid.Width; i++)
            {
                //Set voxel position on the grid
                Entity voxelEntity = em.Instantiate(voxelGrid.VoxelPrefab);
                var voxelTransform = em.GetComponentData<LocalTransform>(voxelEntity);
                voxelTransform.Position = new float3(vgPos.x + voxelGrid.GridOffset * i, vgPos.y, vgPos.z + voxelGrid.GridOffset * lengthBuffer);

                //Set parent
                em.AddComponent<Parent>(voxelEntity);
                var voxelParent = em.GetComponentData<Parent>(voxelEntity);
                voxelParent.Value = voxelGridEntity;

                //Set voxel component data
                Voxel voxel = em.GetComponentData<Voxel>(voxelEntity);
                voxel.Row = lengthBuffer;
                voxel.Column = i;
                //voxel.Height = lengthBuffer < (float)voxelGrid.Length / 3 || lengthBuffer > (float)voxelGrid.Length * 2 / 3 ? (float)voxel.MaxHeight / 2 : voxel.MaxHeight;
                voxel.Height = math.clamp(SimConfigurationController.Instance.carHeightMapGenerator.GetHeight(lengthBuffer, i), voxel.MinHeight, voxel.MaxHeight);
                voxel.VoxelSize = voxelGrid.GridOffset;

                voxel.IsVoxelReady = false;

                em.SetComponentData(voxelEntity, voxelTransform);
                em.SetComponentData(voxelEntity, voxelParent);
                em.SetComponentData(voxelEntity, voxel);
            }
        }
    }
}
