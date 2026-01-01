using System.Collections.Generic;

public class ProjectileAnimationStateManager
{
    public enum ProjectileID
    {
        UNSET = -1,
        BLASTER_BEAM = 0,
        PHASE_BEAM = 1,
    }

    public readonly struct Data
    {
        public readonly string finishStateName;

        public Data(string finishStateName)
        {
            this.finishStateName = finishStateName;
        }
    }

    private readonly Dictionary<ProjectileID, Data> animationDatas = new();

    public ProjectileAnimationStateManager()
    {
        animationDatas.Add(ProjectileID.BLASTER_BEAM, new(BlasterBeamAnimatorStates.BaseLayer.BLASTER_BEAM_DISSIPATE));
        animationDatas.Add(ProjectileID.PHASE_BEAM, new(PhaseBeamAnimatorStates.BaseLayer.PHASE_BEAM_DISSIPATE));
    }

    public Data GetAnimationData(ProjectileID id)
    {
        if (id == ProjectileID.UNSET)
        {
            throw new($"id not set!!");
        }

        if (!animationDatas.ContainsKey(id))
        {
            throw new($"key {id} not found!!");
        }

        return animationDatas[id];
    }

}
