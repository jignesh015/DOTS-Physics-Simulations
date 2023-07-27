using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Rendering;

namespace PhysicsSimulations
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VoxelSystem : ISystem
    {
        bool isDone;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>()
               .CreateCommandBuffer(state.WorldUnmanaged);


            if(SimConfigurationController.Instance != null && SimConfigurationController.Instance.VoxelGridReady)
            {
                JobHandle adjustHeight = new AdjustHeight
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    Ecb = ecb,
                    MaterialMeshComp = SystemAPI.GetComponentLookup<MaterialMeshInfo>(),
                }.Schedule<AdjustHeight>(state.Dependency);

                adjustHeight.Complete();

                //Voxels Ready job
                if(!SimConfigurationController.Instance.VoxelsReady)
                {
                    JobHandle allVoxelsReady = new AllVoxelsReady
                    {
                    }.ScheduleParallel<AllVoxelsReady>(state.Dependency);

                    allVoxelsReady.Complete();
                    SimConfigurationController.Instance.OnVoxelsReady?.Invoke();
                }
                
            }
            
            
            if(TrainingController.Instance != null && TrainingController.Instance.SetNewVoxelHeight)
            {
                //UnityEngine.Debug.Log($"<color=magenta>SetNewVoxelHeight {TrainingController.Instance.VoxelHeightFactorList[0]}</color>");
                JobHandle getNewHeight = new GetNewHeight
                {
                }.ScheduleParallel<GetNewHeight>(state.Dependency);

                getNewHeight.Complete();
            }
        }

        [BurstCompile]
        public partial struct AdjustHeight : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer Ecb;
            public ComponentLookup<MaterialMeshInfo> MaterialMeshComp;

            public void Execute(ref Voxel voxel, ref LocalToWorld localToWorld, ref PhysicsCollider collider, in Entity entity)
            {
                //Debug.Log($"{localToWorld.Value[0]} | {localToWorld.Value[1]} | {localToWorld.Value[2]} |{localToWorld.Value[3]}");
                if (!voxel.IsVoxelReady)
                {
                    if(voxel.Height < voxel.MinHeight)
                    {
                        Ecb.DestroyEntity(entity);
                        return;
                    }

                    if (math.abs(localToWorld.Value[1][1] - voxel.Height) > 0.001f)
                    {
                        //Lerp the Y-scale of the voxel
                        float yScale = math.lerp(localToWorld.Value[1][1], voxel.Height, DeltaTime * 2f);
                        localToWorld.Value[1][1] = yScale;

                        //Lerp the Y-position of the voxel to keep it grounded
                        localToWorld.Value[3][1] = yScale / 2f;

                        ////Lerp the collider scale
                        //collider.Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                        //{
                        //    Center = float3.zero,
                        //    BevelRadius = 0.01f,
                        //    Orientation = quaternion.identity,
                        //    Size = new float3(voxel.VoxelSize, yScale, voxel.VoxelSize)
                        //});
                    }
                    else
                    {
                        //Set the final Y-scale and Y-position
                        localToWorld.Value[1][1] = voxel.Height;
                        localToWorld.Value[3][1] = voxel.Height / 2f;

                        //Scale the collider
                        collider.Value = BoxCollider.Create(new BoxGeometry
                        {
                            Center = float3.zero,
                            BevelRadius = voxel.VoxelSize / 2f,
                            Orientation = quaternion.identity,
                            Size = new float3(voxel.VoxelSize, voxel.Height, voxel.VoxelSize)
                        },
                        //Add collision filter
                        collider.Value.Value.GetCollisionFilter(),
                        //Add mat to raise events
                        new Material
                        {
                            CollisionResponse = CollisionResponsePolicy.CollideRaiseCollisionEvents
                        });

                        //Set appropriate mesh material
                        var materialComponent = MaterialMeshComp[entity];
                        switch (voxel.MatRefIndex)
                        {
                            case 0:
                                materialComponent.Material = MaterialMeshComp[voxel.BaseMatRef].Material;
                                break;
                            case 1:
                                materialComponent.Material = MaterialMeshComp[voxel.PositiveMatRef].Material;
                                break;
                            case 2:
                                materialComponent.Material = MaterialMeshComp[voxel.NegativeMatRef].Material;
                                break;
                        }
                        MaterialMeshComp[entity] = materialComponent;

                        voxel.IsVoxelReady = true;
                    }
                }
            }
        }

        [BurstCompile]
        public partial struct GetNewHeight : IJobEntity
        {
            public readonly void Execute(ref Voxel voxel)
            {
                if(voxel.IsVoxelReady)
                {
                    //float _newHeight = voxel.Height + (TrainingController.Instance.VoxelHeightFactor * TrainingController.Instance.maxVoxelVariance);

                    //Adjust Height factor according to adjacent row's factor
                    float _currentRowFactor = TrainingController.Instance.GetHeightFactor(voxel.Row, voxel.Column);
                    float _maxVoxelHeightVariance = TrainingController.Instance.maxVoxelHeightVariance;
                    float _adjacentRowMaxHeightVariance = TrainingController.Instance.adjacentRowMaxHeightVariance;
                    //if (voxel.Row > 0 && voxel.Column > 0)
                    //{
                    //    float _previousRowFactor = TrainingController.Instance.GetHeightFactor(voxel.Row - 1);
                    //    float _variance = math.abs(_previousRowFactor - _currentRowFactor);

                    //    if (_variance > _adjacentRowMaxHeightVariance)
                    //        _currentRowFactor = math.clamp((_currentRowFactor / _variance), -_adjacentRowMaxHeightVariance, _adjacentRowMaxHeightVariance);
                    //}

                    //Calculate new height as per previous height and adjusted height factor
                    float _newHeight = voxel.Height + (_currentRowFactor * _maxVoxelHeightVariance);

                    //Clamp the new height to be within the acceptable variance of the og height
                    _newHeight = math.clamp(_newHeight, voxel.OgHeight - _maxVoxelHeightVariance, voxel.OgHeight + _maxVoxelHeightVariance);

                    //Check if height increased, decreased or remained same
                    float _heightToCheck = TrainingController.Instance.compareWithOgHeight ? voxel.OgHeight : voxel.Height;
                    if (_newHeight < _heightToCheck)
                        voxel.MatRefIndex = 2;
                    else if(_newHeight > _heightToCheck)
                        voxel.MatRefIndex = 1;
                    else
                        voxel.MatRefIndex = 0;

                    //Make sure the height is within limit
                    voxel.Height = math.clamp(_newHeight, voxel.MinHeight, voxel.MaxHeight);
                    voxel.IsVoxelReady = false;
                }
                SimConfigurationController.Instance.VoxelsReady = false;
                TrainingController.Instance.SetNewVoxelHeight = voxel.IsVoxelReady;
            }
        }

        [BurstCompile]
        public partial struct AllVoxelsReady : IJobEntity
        {
            public readonly void Execute(ref Voxel voxel)
            {
                SimConfigurationController.Instance.VoxelsReady = voxel.IsVoxelReady;
            }
        }
    }
}
